using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using WorldGeneratorFunctionalTests;

namespace WorldGenerator
{
    public enum RenderMode
    {
        Perspective,
        Section
    }

    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;

        private Effect? _worldEffect;
        private RenderMesh? _worldMesh;
        private Matrix _world = Matrix.CreateTranslation(0, 0, 0);
        private Matrix _view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        private Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        private TextureCube? _globeTexture;
        private TextureCube? _normalTexture;
        private Texture2D? _pointTexture;
        private const int _cubeTexSize = 1024;
        private const float _initialZoom = 3300;
        private float _zoomFactor = _initialZoom;
        private const float _mouseSensitivity = 0.01f;
        private int _width;
        private int _height;
        private SpriteFont? _font;

        private int _frameCount = -1;

        private RenderMode _renderMode = RenderMode.Perspective;

        private IReadOnlyList<FunctionalTest> _tests =
            new List<FunctionalTest>
            {
                new LargeForcesStretchPlate(),
                new PlateStretchesAtWeakPoint(),
                new SlidesWithGravity(),
                new SlidesFastOnSteepSlope(),
                new DoesntSlideOnFlat(),
                new LithosphereCoolsOverTime(),
                new OldPlateSubductsSpontaneously(),
                new DiscontinuityInducesSubduction(),
                new RollbackCreatesContinent(),
                new SubductionCreatesArcs(),
                new CollisionRaisesMountains(),
                new FlatSubductionRaisesPlateau(),
                new SteepSubductionCreatesBackArc(),
                new MountainsCauseFlexing(),
                new SpreadingCreatesRidges(),
                new WilsonCycle(),
                new Globe(),
            };
        private TestResult? _status;
        private int _testIndex = -1;
        private List<TestResult> _results = new();
        private FunctionalTest _currentTest;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
            _currentTest = _tests.First();
        }

        protected override void Initialize()
        {
            _graphics.GraphicsProfile = GraphicsProfile.HiDef;

            _graphics.PreferredBackBufferHeight = GraphicsDevice.DisplayMode.Height;
            _graphics.PreferredBackBufferWidth = GraphicsDevice.DisplayMode.Width;
            _graphics.ApplyChanges();

            _worldEffect = Content.Load<Effect>("WorldEffect");
            _worldEffect.CurrentTechnique = _worldEffect.Techniques[0];
            _globeTexture = new TextureCube(GraphicsDevice, _cubeTexSize, true, SurfaceFormat.Color);
            _normalTexture = new TextureCube(GraphicsDevice, _cubeTexSize, true, SurfaceFormat.Vector4);

            _pointTexture = Texture2D.FromFile(GraphicsDevice, "Content/blank.png");

            _width = _graphics.PreferredBackBufferWidth;
            _height = _graphics.PreferredBackBufferHeight;
            SetNextTest();

            _font = Content.Load<SpriteFont>("File");

            base.Initialize();
        }

        private void SetNextTest()
        {
            if(_status is not null &&
                _status.OverallState is not Running)
            {
                _results[0] = _status;
            }

            _testIndex++;
            if (_testIndex >= _tests.Count) return;
            try
            {
                var mesh = new Mesh(_tests[_testIndex].Faces, _tests[_testIndex].Vertices.ToList());
                _worldMesh = new RenderMesh(mesh, GraphicsDevice);
            }
            catch { }

            _currentTest = _tests[_testIndex]; 
            _results.Insert(0, new( new Running(_currentTest.Name), Enumerable.Empty<State>(), 0, 0));

            _status = null;
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var timestep = new TimeKY(1);

            if (_status?.OverallState is not Running and not null) return;

            if (_frameCount % 10 == 0)
            {
                try
                {
                    _currentTest.Update(gameTime);
                    _currentTest.PostUpdate();
                    _status = _currentTest.Evaluate();
                    _results[0] = _status;

                    if (_status.OverallState is not Running) SetNextTest();
                }
                catch (Exception e)
                {
                    _status = new(
                        new Failed(_currentTest.Name + ", threw: " + e.Message),
                        Enumerable.Empty<State>(), -1, -1);
                    _results[0] = _status;

                    SetNextTest();
                }
            }
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _frameCount++;

            GraphicsDevice.Clear(Color.Black);

            var cameraLoc = AimCamera();

            Action<Vector3> renderFunc = _renderMode switch
            {
                RenderMode.Perspective => DrawPerspective,
                RenderMode.Section => DrawPerspective,
                _ => throw new NotImplementedException(),
            };

            renderFunc(cameraLoc);

            _spriteBatch?.Begin();

            var status = _results.SelectMany(r =>
                (r.OverallState switch
                {
                    Running running => new[] { ($"{running.Name}: Running, Frame: {r.Frame} / {r.TotalFrames}", Color.White, 0) },
                    Succeeded success => new[] { ($"{success.Name}: Succeeded", Color.Green, 0) },
                    Failed failure => new[] { ($"{failure.Name}: Failed", Color.Red, 0) },
                    _ => throw new NotImplementedException(),
                }).Concat(r.SubStates.Select(s =>
                (s switch
                {
                        Running => ($"{s.Name}", Color.Yellow, 1),
                        Succeeded => ($"{s.Name}", Color.Green, 1),
                        Failed => ($"{s.Name}", Color.Red, 1),
                        _ => throw new NotImplementedException(),
                    }))));

            int itemId = 0;
            foreach(var s in status)
            {
                _spriteBatch?.DrawString(_font, s.Item1, new Vector2(10 + 40 * s.Item3, 50 + 20 * itemId + 1), s.Item2);
                itemId++;
            }

            DrawGraph(_currentTest.SeriesData);

            _spriteBatch?.End();

            base.Draw(gameTime);
        }

        private void DrawGraph(IReadOnlyList<float> seriesData)
        {
            if(!seriesData.Any()) return;

            var max = seriesData.Max();
            var min = seriesData.Min();

            var range = max - min;

            var graphHeight = 100;
            
            var normalisedData = seriesData.Select(d => (d - min) / range);

            foreach(var (data, index) in normalisedData.Select((d, i) => (d, i)))
            {
                var x = 500 + index * 5;
                var y = 100 + (1 - data) * graphHeight;

                _spriteBatch?.Draw(_pointTexture, new Rectangle(x, (int)y, 1, 1), Color.White);
            }
        }

        private void DrawPerspective(Vector3 cameraLoc)
        {
            try
            {
                _worldMesh?.SetVertices(_currentTest.Vertices, _currentTest.Colors.Values, GraphicsDevice);
            }
            catch { }

            _view = Matrix.CreateLookAt(cameraLoc, Vector3.Zero, Vector3.UnitY);

            var wvp = _world * _view * _projection;

            _worldEffect?.Parameters["WorldViewProjection"].SetValue(wvp);
            _worldEffect?.Parameters["CameraPos"].SetValue(cameraLoc);
            //_worldEffect?.Parameters["GlobeTexture"].SetValue(_globeTexture);
            // _worldEffect.Parameters["NormalTexture"].SetValue(_normalTexture);

            GraphicsDevice.SetVertexBuffer(_worldMesh?.VertexBuffer);
            GraphicsDevice.Indices = _worldMesh?.IndexBuffer;

            var rasterizerState = new RasterizerState
            {
                // Draws the back of the cube because the raycast works out
                // the same and the back can't clip through the front plane
                CullMode = CullMode.CullClockwiseFace
            };

            rasterizerState.FillMode = FillMode.WireFrame;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in _worldEffect?.CurrentTechnique.Passes ?? Enumerable.Empty<EffectPass>())
            {
                pass.Apply();
                if (_worldMesh == null)
                {
                    continue;
                }
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _worldMesh.FacesCount);
            }
        }

        private Vector3 AimCamera()
        {
            _zoomFactor = _initialZoom - Mouse.GetState().ScrollWheelValue / 10.0f;

            var cameraLoc = new Vector3(0.0f, 0.0f, _zoomFactor / 200.0f);

            var mousePosY = (Mouse.GetState().Y - (_height * 0.5f)) * _mouseSensitivity;
            mousePosY = MathF.Min(2.0f, MathF.Max(-2.0f, mousePosY));

            var xRot = Matrix.CreateRotationX(mousePosY);
            cameraLoc = Vector3.Transform(cameraLoc, xRot);

            var yRot = Matrix.CreateRotationY(Mouse.GetState().X * _mouseSensitivity);
            cameraLoc = Vector3.Transform(cameraLoc, yRot);
            return cameraLoc;
        }
    }
}