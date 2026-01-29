using ImageLoadModule;
using PopupModule;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace Bootstrap
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private int maxParallelDownloads = 4;
        
        
        protected override void Configure(IContainerBuilder builder)
        {

            builder.Register(_ => 
                new ImageDownloadService(maxParallelDownloads), Lifetime.Singleton).AsImplementedInterfaces();
            builder.Register<UIPopupService>(Lifetime.Singleton).AsImplementedInterfaces().AsSelf();
        }
    }
}