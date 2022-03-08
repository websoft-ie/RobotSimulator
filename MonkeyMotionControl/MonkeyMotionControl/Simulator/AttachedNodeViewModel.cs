using HelixToolkit.Wpf.SharpDX;
using HelixToolkit.Wpf.SharpDX.Model.Scene;
using System.ComponentModel;

namespace MonkeyMotionControl.Simulator
{
    public class AttachedNodeViewModel : INotifyPropertyChanged
    {

        /// INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        //////////
        
        private SceneNode node;

        private bool selected = false;
        public bool Selected
        {
            set
            {
                selected = value;
                if (node is MeshNode m)
                {
                    if (HighlightEnable)
                    {
                        m.PostEffects = value ? $"highlight[color:#FFFF00]" : "";
                    }
                    foreach (var n in node.TraverseUp())
                    {
                        if (n.Tag is AttachedNodeViewModel vm)
                        {
                            vm.Expanded = true;
                        }
                    }
                }
                OnPropertyChanged(nameof(Selected));
            }
            get => selected;
        }

        private bool expanded = false;
        public bool Expanded
        {
            set
            {
                expanded = value;
                OnPropertyChanged(nameof(Expanded));
            }
            get => expanded;
        }

        public bool IsAnimationNode { get => node.IsAnimationNode; }

        public bool HighlightEnable { get; set; } = false;

        public string Name { get => node.Name; }

        public AttachedNodeViewModel(SceneNode node)
        {
            this.node = node;
            node.Tag = this;
        }
    }
}
