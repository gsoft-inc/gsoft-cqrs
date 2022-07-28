using Microsoft.Extensions.DependencyInjection;

namespace GSoft.Cqrs.Tests;

public abstract class BaseServiceCollectionTest : IDisposable
{
    private readonly IServiceCollection _serviceCollection = new ServiceCollection();

    private ServiceProvider? _serviceProvider;

    public IServiceCollection Services
    {
        get
        {
            if (this._serviceProvider == null)
            {
                return this._serviceCollection;
            }

            throw new InvalidOperationException("Can't access services once you created the provider.");
        }
    }

    public IServiceProvider Provider
    {
        get
        {
            return this._serviceProvider ??= this._serviceCollection.BuildServiceProvider();
        }
    }

    public void Dispose()
    {
        this.Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            this._serviceProvider?.Dispose();
        }
    }
}