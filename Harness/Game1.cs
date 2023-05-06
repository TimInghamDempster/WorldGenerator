using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WorldGenerator
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch? _spriteBatch;

        private Effect? _worldEffect;
        private Mesh? _cube;
        private Matrix _world = Matrix.CreateTranslation(0, 0, 0);
        private Matrix _view = Matrix.CreateLookAt(new Vector3(0, 0, 3), new Vector3(0, 0, 0), new Vector3(0, 1, 0));
        private Matrix _projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(45), 800f / 480f, 0.01f, 100f);
        private TextureCube? _globeTexture;
        private TextureCube? _normalTexture;
        private const int _cubeTexSize = 1024;
        private const float _initialZoom = 500;
        private float _zoomFactor = _initialZoom;
        private const float _maxYRange = 0.5f;
        private const float _mouseSensitivity = 0.01f;
        private int _width;
        private int _height;

        private readonly IManifold _manifold;
        private readonly DistToNearestPointField _field;
        private readonly DistToGrayscaleVisualiser _visualiser = new();

        private int _frameCount = -1;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;

            _manifold = new PointCloudManifold(
                SphereLoader.LoadSphere());
            _field = new(_manifold);
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

            _cube = Mesh.Cube(GraphicsDevice, 1.1f);

            base.Initialize();
        }


        private void DrawGlobeTexture()
        {
            if (_frameCount == 0)
            {
                var faces = new byte[6][];
                var normalFaces = new Vector4[6][];
                for (int i = 0; i < faces.Length; i++)
                {
                    normalFaces[i] = new Vector4[_cubeTexSize * _cubeTexSize];
                }

                for (int i = 0; i < 6; i++) faces[i] = DrawGlobeFace(i, normalFaces[i]);

                //Parallel.For(0, 6, i => faces[i] = DrawGlobeFace(i, normalFaces[i]));

                _globeTexture = new TextureCube(GraphicsDevice, _cubeTexSize, true, SurfaceFormat.Color);


                for (int i = 0; i < faces.Length; i++)
                {
                    _globeTexture?.SetData((CubeMapFace)i, faces[i]);
                    _normalTexture?.SetData((CubeMapFace)i, normalFaces[i]);
                }
            }
        }

        private record FaceGeometry(Vector3 Centre, Vector3 Offset1, Vector3 Offset2, CubeMapFace Face);
        private byte[] DrawGlobeFace(int face, Vector4[] normals)
        {
            var geometry = CalcGeometry(face);

            return WriteColourMap(normals, geometry);
        }

        private byte[] WriteColourMap(Vector4[] normals, FaceGeometry geometry)
        {
            byte[] colourBuffer = new byte[_cubeTexSize * _cubeTexSize * 4];
            //for (int y = 0; y < _cubeTexSize; y++)
            Parallel.For(0, _cubeTexSize, y =>
            {
                for (int x = 0; x < _cubeTexSize; x++)
                {
                    var dx = x - _cubeTexSize / 2;
                    var dy = y - _cubeTexSize / 2;

                    var dir = geometry.Centre + dx * geometry.Offset1 + dy * geometry.Offset2;
                    dir = Vector3.Normalize(dir);

                    var colour = _visualiser.GetColour(new(dir, Unit.None), _field);

                    var normalIndex = (x + y * _cubeTexSize);
                    var baseIndex = normalIndex * 4;
                    colourBuffer[baseIndex + 0] = colour.R;
                    colourBuffer[baseIndex + 1] = colour.G;
                    colourBuffer[baseIndex + 2] = colour.B;
                    colourBuffer[baseIndex + 3] = 255;
                }
            });
            return colourBuffer;
        }

        private FaceGeometry CalcGeometry(int face)
        {
            var typedFace = (CubeMapFace)face;
            var faceNormal = typedFace switch
            {
                CubeMapFace.PositiveX => Vector3.UnitX,
                CubeMapFace.NegativeX => -Vector3.UnitX,
                CubeMapFace.PositiveY => -Vector3.UnitY,
                CubeMapFace.NegativeY => Vector3.UnitY,
                CubeMapFace.PositiveZ => Vector3.UnitZ,
                CubeMapFace.NegativeZ => -Vector3.UnitZ,
                _ => throw new NotImplementedException(),
            };
            var offsetAmount = 2.0f / _cubeTexSize;
            var offset1 = typedFace switch
            {
                CubeMapFace.PositiveX => new Vector3(0.0f, 0.0f, -offsetAmount),
                CubeMapFace.NegativeX => new Vector3(0.0f, 0.0f, offsetAmount),
                CubeMapFace.PositiveY => new Vector3(offsetAmount, 0.0f, 0.0f),
                CubeMapFace.NegativeY => new Vector3(offsetAmount, 0.0f, 0.0f),
                CubeMapFace.PositiveZ => new Vector3(offsetAmount, 0.0f, 0.0f),
                CubeMapFace.NegativeZ => new Vector3(-offsetAmount, 0.0f, 0.0f),
                _ => throw new NotImplementedException(),
            };
            var offset2 = typedFace switch
            {
                CubeMapFace.PositiveX => new Vector3(0.0f, offsetAmount, 0.0f),
                CubeMapFace.NegativeX => new Vector3(0.0f, offsetAmount, 0.0f),
                CubeMapFace.PositiveY => new Vector3(0.0f, 0.0f, offsetAmount),
                CubeMapFace.NegativeY => new Vector3(0.0f, 0.0f, -offsetAmount),
                CubeMapFace.PositiveZ => new Vector3(0.0f, offsetAmount, 0.0f),
                CubeMapFace.NegativeZ => new Vector3(0.0f, offsetAmount, 0.0f),
                _ => throw new NotImplementedException(),
            };

            return new(faceNormal, offset1, offset2, typedFace);
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

            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            _frameCount++;
            DrawGlobeTexture();

            GraphicsDevice.Clear(Color.Black);

            _zoomFactor = 500 - Mouse.GetState().ScrollWheelValue / 10.0f;

            var cameraLoc = new Vector3(0.0f, 0.0f, _zoomFactor / 200.0f);

            var mousePosY = (Mouse.GetState().Y - (_height * 0.5f)) * _mouseSensitivity;
            mousePosY = MathF.Min(2.0f, MathF.Max(-2.0f, mousePosY));

            var xRot = Matrix.CreateRotationX(mousePosY);
            cameraLoc = Vector3.Transform(cameraLoc, xRot);

            var yRot = Matrix.CreateRotationY(Mouse.GetState().X * _mouseSensitivity);
            cameraLoc = Vector3.Transform(cameraLoc, yRot);

            _view = Matrix.CreateLookAt(cameraLoc, Vector3.Zero, Vector3.UnitY);

            var wvp = _world * _view * _projection;

            _worldEffect?.Parameters["WorldViewProjection"].SetValue(wvp);
            _worldEffect?.Parameters["CameraPos"].SetValue(cameraLoc);
            _worldEffect?.Parameters["GlobeTexture"].SetValue(_globeTexture);
            // _worldEffect.Parameters["NormalTexture"].SetValue(_normalTexture);

            GraphicsDevice.SetVertexBuffer(_cube?.VertexBuffer);
            GraphicsDevice.Indices = _cube?.IndexBuffer;

            var rasterizerState = new RasterizerState
            {
                // Draws the back of the cube because the raycast works out
                // the same and the back can't clip through the front plane
                CullMode = CullMode.CullClockwiseFace
            };

            rasterizerState.FillMode = FillMode.Solid;
            GraphicsDevice.RasterizerState = rasterizerState;

            foreach (EffectPass pass in _worldEffect?.CurrentTechnique.Passes ?? Enumerable.Empty<EffectPass>())
            {
                pass.Apply();
                GraphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _cube?.Faces.Count ?? 0);
            }

            base.Draw(gameTime);
        }
    }
}