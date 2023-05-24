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
        private const int _cubeTexSize = 1024;
        private const float _initialZoom = 500;
        private float _zoomFactor = _initialZoom;
        private const float _mouseSensitivity = 0.01f;
        private int _width;
        private int _height;
        private SpriteFont? _font;

        private int _frameCount = -1;

        private RenderMode _renderMode = RenderMode.Perspective;

        private IReadOnlyList<IFunctionalTest> _tests =
            new List<IFunctionalTest>
            {
                new GravityTest(),
                new BouyancyTestsFloats(),
                new BouyancyTestsSinks(),
            };
        private State _status = new Running();
        private int _testIndex = -1;
        private List<State> _results = new();
        private IFunctionalTest _currentTest;

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


            _width = _graphics.PreferredBackBufferWidth;
            _height = _graphics.PreferredBackBufferHeight;
            SetNextTest();

            _font = Content.Load<SpriteFont>("File");

            base.Initialize();
        }

        private void SetNextTest()
        {
            if(_status is not null and not Running)
            {
                _results.Add(_status);
            }

            _testIndex++;
            if (_testIndex >= _tests.Count) return;

            var mesh = new Mesh(_tests[_testIndex].Faces, _tests[_testIndex].Vertices.ToList());
            _worldMesh = new RenderMesh(mesh, GraphicsDevice);
            _currentTest = _tests[_testIndex];
            _status = new Running();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            // TODO: use this.Content to load your game content here
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            var timestep = new Time(1);

            if (_status is not Running) return;

            _status = _currentTest.Update(gameTime);

            if (_status is not Running) SetNextTest();

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

            for (int i = 0; i < _results.Count; i++)
            {
                (string message, Color col) status = _results[i] switch
                {
                    Running => ("Running", Color.White),
                    Succeeded success => ($"{success.Name}: Succeeded", Color.Green),
                    Failed failure => ($"{failure.Name}: Failed, {failure.Error}", Color.Red),
                    _ => throw new NotImplementedException(),
                };

                _spriteBatch?.Begin();

                _spriteBatch?.DrawString(_font, status.message, new Vector2(10, 10 + 20*i), status.col);

                _spriteBatch?.End();
            }

            base.Draw(gameTime);
        }

       

        private void DrawPerspective(Vector3 cameraLoc)
        {
            _worldMesh?.SetVertices(_currentTest.Vertices, GraphicsDevice);

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
            _zoomFactor = 500 - Mouse.GetState().ScrollWheelValue / 10.0f;

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