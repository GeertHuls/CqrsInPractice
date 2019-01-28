using CSharpFunctionalExtensions;

namespace Logic.AppServices
{
    public interface ICommandHandler<in TCommand> where TCommand : ICommand
    {
        Result Handle(TCommand command);
    }
}