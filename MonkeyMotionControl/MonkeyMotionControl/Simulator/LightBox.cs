using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using SharpDX;

namespace MonkeyMotionControl.Simulator
{
    class LightBox : MeshGeometryModel3D
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

        public LightBox(Vector3 center)
            : base()
        {
            var builder = new MeshBuilder();
            builder.AddBox(Vector3.Zero, 100, 1000, 10, BoxFaces.All);
            Geometry = builder.ToMeshGeometry3D();
            CullMode = SharpDX.Direct3D11.CullMode.Back;
           
            var LightMaterial = new PhongMaterial
            {
                AmbientColor = Color.Gray,
                DiffuseColor = Color.Gray,
                EmissiveColor = Color.White,
                SpecularColor = Color.Black
            };

            var material = LightMaterial;
            Material = material;

            if (material.DiffuseColor.Alpha < 1)
            {
                IsTransparent = true;
            }

            Position = center;
            SceneNode.Name = "Light Box";
        }

        private void UpdateModelMatrix()
        {
            SceneNode.ModelMatrix = Matrix.Translation(-centerOffset) * scaleMatrix * rotationMatrix * Matrix.Translation(centerOffset) * Matrix.Translation(translationVector);
        }
    }
}
