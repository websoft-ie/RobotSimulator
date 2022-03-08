using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using MonkeyMotionControl.UI;
using SharpDX;

namespace MonkeyMotionControl.Simulator
{
    class PlaneFloor3D : MeshGeometryModel3D
    {
        public Vector3 Position
        {
            get
            {
                return SceneNode.ModelMatrix.TranslationVector;
            }
            set
            {
                translationVector = value;
                UpdateModelMatrix();
            }
        }

        private Vector3 centerOffset = Vector3.Zero;
        private Vector3 translationVector = Vector3.Zero;
        private Matrix rotationMatrix = Matrix.Identity;
        private Matrix scaleMatrix = Matrix.Identity;

        public PlaneFloor3D()
            : base()
        {

        }

        public PlaneFloor3D(Vector3 center)
            : base()
        {
            var builder = new MeshBuilder(true,true,true);
            builder.AddBox(Vector3.Zero, 10000, 10000, 10, BoxFaces.Top);
            Geometry = builder.ToMeshGeometry3D();
            CullMode = SharpDX.Direct3D11.CullMode.Back;
            
            var FloorMaterial = new PhongMaterial
            {
                AmbientColor = Color.Gray,
                DiffuseColor = new Color4(0.01f, 0.01f, 0.01f, 0.2f),
                SpecularColor = Color.White,
                SpecularShininess = 200f,
                DiffuseMap = Texture.LoadFileToMemory(new System.Uri("./Resources/Sim_TextureFloorDiffuse.jpg", System.UriKind.RelativeOrAbsolute).ToString()),
                NormalMap = Texture.LoadFileToMemory(new System.Uri("./Resources/Sim_TextureFloorNormal.jpg", System.UriKind.RelativeOrAbsolute).ToString()),
                RenderShadowMap = true,
                RenderDiffuseMap = true,
                RenderNormalMap = true,
            };
            var material = FloorMaterial;
            Material = material;

            if (material.DiffuseColor.Alpha < 1)
            {
                IsTransparent = true;
            }

            Position = center;
            SceneNode.Name = "Floor Model";
        }

        private void UpdateModelMatrix()
        {
            SceneNode.ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
        }

    }
}
