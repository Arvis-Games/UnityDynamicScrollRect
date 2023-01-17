using dynamicscroll;
using pooling;
using Zenject;

namespace Arvis.BoardRoyaleDuels
{
    public class MenuInstaller : MonoInstaller
    {
        public override void InstallBindings()
        {
            Container.Bind(typeof(DynamicScroll<,>)).AsTransient();
            Container.Bind(typeof(Pooling<>)).AsTransient().WithArguments(Container);
        }
    }
}
