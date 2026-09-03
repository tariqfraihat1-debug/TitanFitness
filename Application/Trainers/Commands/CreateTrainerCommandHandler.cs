using CSharpFunctionalExtensions;
using MediatR;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Common.ValueObjects;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Trainers.Commands;

public sealed class CreateTrainerCommandHandler
    : IRequestHandler<CreateTrainerCommand, Result<int, Error>>
{
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;
    private readonly IWriteRepository<Trainer> _trainerWriteRepository;
    private readonly IUnitOfWork _unitOfWork;

    public CreateTrainerCommandHandler(
        IReadOnlyRepository<Branch, int> branchReadRepository,
        IWriteRepository<Trainer> trainerWriteRepository,
        IUnitOfWork unitOfWork)
    {
        _branchReadRepository = branchReadRepository;
        _trainerWriteRepository = trainerWriteRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<Result<int, Error>> Handle(
        CreateTrainerCommand request,
        CancellationToken cancellationToken)
    {
        // Validate the selected branch.
        bool branchExists = await _branchReadRepository.AnyAsync(
            branch => branch.Id == request.Trainer.BranchId,
            cancellationToken);

        if (!branchExists)
            return Result.Failure<int, Error>(Error.EntityNotFound(nameof(Branch), request.Trainer.BranchId));

        // Create the optional contact value objects.
        Email? email = null;

        if (!string.IsNullOrWhiteSpace(request.Trainer.Email))
        {
            Result<Email, Error> emailResult = Email.Create(request.Trainer.Email);

            if (emailResult.IsFailure)
                return Result.Failure<int, Error>(emailResult.Error);

            email = emailResult.Value;
        }

        Phone? phone = null;

        if (!string.IsNullOrWhiteSpace(request.Trainer.Phone))
        {
            Result<Phone, Error> phoneResult = Phone.Create(request.Trainer.Phone);

            if (phoneResult.IsFailure)
                return Result.Failure<int, Error>(phoneResult.Error);

            phone = phoneResult.Value;
        }

        // Let the Domain validate and create the trainer.
        Result<Trainer, Error> trainerResult = Trainer.Create(
            request.Trainer.Name,
            request.Trainer.BranchId,
            request.Trainer.Specialty,
            email,
            phone,
            request.Trainer.IsActive);

        if (trainerResult.IsFailure)
            return Result.Failure<int, Error>(trainerResult.Error);

        // Persist the new trainer.
        await _trainerWriteRepository.AddAsync(trainerResult.Value, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return Result.Success<int, Error>(trainerResult.Value.Id);
    }
}