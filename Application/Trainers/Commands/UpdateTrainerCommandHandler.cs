using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Trainers.Commands;

public sealed class UpdateTrainerCommandHandler
    : IRequestHandler<UpdateTrainerCommand, UnitResult<Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IWriteRepository<Trainer> _trainerWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public UpdateTrainerCommandHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IWriteRepository<Trainer> trainerWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository;
        _trainerWriteRepository = trainerWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<UnitResult<Error>> Handle(
        UpdateTrainerCommand request,
        CancellationToken cancellationToken)
    {
        // Load the tracked trainer and validate the selected branch.
        var data = await _trainerWriteRepository
            .Query()
            .Where(trainer => trainer.Id == request.TrainerId)
            .Select(trainer => new
            {
                Trainer = trainer,
                BranchExists = _branchReadRepository
                    .GetAll()
                    .Any(branch => branch.Id == request.Trainer.BranchId)
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (data is null)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Trainer), request.TrainerId));

        if (!data.BranchExists)
            return UnitResult.Failure(Error.EntityNotFound(nameof(Branch), request.Trainer.BranchId));

        // Create the optional contact value objects.
        Email? email = null;

        if (!string.IsNullOrWhiteSpace(request.Trainer.Email))
        {
            Result<Email, Error> emailResult = Email.Create(request.Trainer.Email);

            if (emailResult.IsFailure)
                return UnitResult.Failure(emailResult.Error);

            email = emailResult.Value;
        }

        Phone? phone = null;

        if (!string.IsNullOrWhiteSpace(request.Trainer.Phone))
        {
            Result<Phone, Error> phoneResult = Phone.Create(request.Trainer.Phone);

            if (phoneResult.IsFailure)
                return UnitResult.Failure(phoneResult.Error);

            phone = phoneResult.Value;
        }

        // Let the Domain validate and update the trainer.
        UnitResult<Error> updateResult = data.Trainer.Update(
            request.Trainer.Name,
            request.Trainer.BranchId,
            request.Trainer.Specialty,
            email,
            phone,
            request.Trainer.IsActive);

        if (updateResult.IsFailure)
            return updateResult;

        // Persist the tracked trainer changes.
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return UnitResult.Success<Error>();
    }
}