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

    public record TestResult(State OverallState, IEnumerable<State> SubStates, int Frame);

    public class FunctionalTest
    {
        public IReadOnlyList<Face> Faces => 
            _mesh?.Faces ?? throw new NotImplementedException();
        public IEnumerable<Vector3> Vertices =>
            _manifold?.Values ?? throw new NotImplementedException();
        public void Update(GameTime gameTime)
        {
            var time = new TimeKY(1);
            _fieldGroup?.ProgressTime(time);
            FrameCount++;
        }

        protected TestCriteria? _criteria;
        protected Mesh? _mesh;
        protected IManifold? _manifold;
        protected FieldGroup? _fieldGroup;
        public string Name =>
            GetType().Name.Replace('_', ' ');

        public TestResult Evaluate()
        {
            if (_criteria is null) throw new InvalidOperationException("Test has not been initialized");

            var frameCount = 0;
            var states = new List<(State, ICondition)>();
            frameCount = FrameCount;

            foreach (var condition in _criteria.Conditions)
            {
                states.Add(new(condition.Evaluate(), condition));
            }

            var overallState =
                states.Any(static s => s.Item1 is Failed) ?
                (State)new Failed(Name) :
                states.All(s => s.Item1 is Succeeded ||
                (s.Item2 is ShouldNot && _criteria.TimeoutResult == TimeoutResult.TimedOut)) ?
                new Succeeded(Name) :
                new Running(Name);

            var timedOut = frameCount > _criteria.TimeoutFrames;
            if (timedOut)
            {
                overallState = _criteria.TimeoutResult switch
                {
                    TimeoutResult.TimedOut => new Failed("Test timed out"),
                    TimeoutResult.Completed => new Succeeded(Name),
                    _ => throw new NotImplementedException()
                };
            }
            else if(_criteria.TimeoutResult == TimeoutResult.Completed)
            {
                overallState = new Running(Name);
            }

            return new TestResult(overallState, states.Select(s => overallState is Running ?
            s.Item1 :
            s switch
            {
                (Running, ShouldNot) => new Succeeded(s.Item1.Name),
                (_, ShouldNot) => s.Item1,
                (Running, Should) => new Failed(s.Item1.Name),
                (_, Should) => s.Item1,
                (_, _) => throw new NotImplementedException()
            }),
            FrameCount);
        }

        public int FrameCount { get; private set; } = 0;
    }
}
