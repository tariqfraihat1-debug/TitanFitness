using CSharpFunctionalExtensions;
using MediatR;
using Microsoft.EntityFrameworkCore;
using TitanFitness.Application.Trainers.Contract;
using TitanFitness.Domain.Branches;
using TitanFitness.Domain.Common.Errors;
using TitanFitness.Domain.Trainers;
using TitanFitness.Infrastructure.Repositories.Interfaces;

namespace TitanFitness.Application.Trainers.Queries;

public sealed class GetTrainerByIdQueryHandler
    : IRequestHandler<GetTrainerByIdQuery, Result<TrainerDetails, Error>>
{
    private readonly IReadOnlyRepository<Trainer, int> _trainerReadRepository;
    private readonly IReadOnlyRepository<Branch, int> _branchReadRepository;

    public GetTrainerByIdQueryHandler(
        IReadOnlyRepository<Trainer, int> trainerReadRepository,
        IReadOnlyRepository<Branch, int> branchReadRepository)
    {
        _trainerReadRepository = trainerReadRepository;
        _branchReadRepository = branchReadRepository;
    }

    public async Task<Result<TrainerDetails, Error>> Handle(
        GetTrainerByIdQuery request,
        CancellationToken cancellationToken)
    {
        // Load the trainer with the name of their branch.
        Maybe<TrainerDetails> trainerDetails = await (
            from trainer in _trainerReadRepository.GetAll()
            join branch in _branchReadRepository.GetAll()
                on trainer.BranchId equals branch.Id
            where trainer.Id == request.TrainerId
            select new TrainerDetails(
                trainer.Id,
                trainer.Name,
                trainer.BranchId,
                branch.Name,
                trainer.Specialty,
                trainer.Email != null ? trainer.Email.Value : null,
                trainer.Phone != null ? trainer.Phone.Value : null,
                trainer.IsActive))
            .FirstOrDefaultAsync(cancellationToken);

        // Return not found when the trainer does not exist.
        if (trainerDetails.HasNoValue)
            return Result.Failure<TrainerDetails, Error>(Error.EntityNotFound(nameof(Trainer), request.TrainerId));

        return Result.Success<TrainerDetails, Error>(trainerDetails.Value);
    }
}