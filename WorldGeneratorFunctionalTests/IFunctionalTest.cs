using Microsoft.Xna.Framework;
using WorldGenerator;

namespace WorldGeneratorFunctionalTests
{
    public record State(string Name);
    public record Running(string Name) : State(Name);
    public record Succeeded(string Name) : State(Name);
    public record Failed(string Name) : State(Name);

    public interface ICondition
    {
        State Evaluate();
    }

    public class Should : ICondition
    {
        private readonly Func<bool> _isInState;
        private readonly string _name;

        public Should(Func<bool> isInState, string name)
        {
            _isInState = isInState;
            _name = name;
        }

        public State Evaluate()
        {
            return _isInState() ? new Succeeded(_name) : new Running(_name);
        }
    }

    public class ShouldNot : ICondition
    {
        private readonly Func<bool> _isInState;
        private readonly string _name;

        public ShouldNot(Func<bool> isInState, string name)
        {
            _isInState = isInState;
            _name = name;
        }

        public State Evaluate()
        {
            return _isInState() ?  new Failed(_name) : new Running(_name);
        }
    }

    public enum TimeoutResult
    {
        TimedOut,
        Completed
    }

    public record TestCriteria(
        int TimeoutFrames, 
        TimeoutResult TimeoutResult,
        IEnumerable<ICondition> Conditions);

    public record TestResult(State OverallState, IEnumerable<State> SubStates);

    public static class TestExtensions
    {
        public static string Name(this IFunctionalTest test) => 
            test.GetType().Name.Replace('_', ' ');
        public static TestResult Evaluate(this IFunctionalTest test)
        {
            var frameCount = 0;
            var states = new List<(State, ICondition)>();
            frameCount = test.FrameCount;

            foreach(var condition in test.Criteria.Conditions)
            {
                states.Add(new(condition.Evaluate(), condition));
            }

            var overallState = 
                states.Any(static s => s.Item1 is Failed) ?
                (State)new Failed(test.Name()) :
                states.All(s => s.Item1 is Succeeded ||
                (s.Item2 is ShouldNot && test.Criteria.TimeoutResult == TimeoutResult.TimedOut)) ?
                new Succeeded(test.Name()) :
                new Running(test.Name());

            var timedOut = frameCount > test.Criteria.TimeoutFrames;
            if (timedOut)
            {
                overallState = test.Criteria.TimeoutResult switch
                {
                    TimeoutResult.TimedOut => new Failed("Test timed out"),
                    TimeoutResult.Completed => new Succeeded(test.Name()),
                    _ => throw new NotImplementedException()
                };
            }

            return new TestResult(overallState, states.Select(s => overallState is Running ? 
            s.Item1 :
            s switch
            {
                (Running, ShouldNot) => new Succeeded(s.Item1.Name),
                (_, ShouldNot) => s.Item1,
                (Running, Should) => new Failed(s.Item1.Name),
                (_, Should) => s.Item1,
                (_,_) => throw new NotImplementedException()
            }));
        }
    }

    public interface IFunctionalTest
    {
        IReadOnlyList<Face> Faces { get; }
        IEnumerable<Vector3> Vertices { get; }
        void Update(GameTime gameTime);
        TestCriteria Criteria { get; }
        int FrameCount { get; }
    }
}
