namespace core.services.serilogging.models;

public class 
CompositeDisposable : IDisposable {
  public 
  CompositeDisposable(IEnumerable<IDisposable> items) {
    items_ = items;
  }

  public void 
  Dispose() {
    foreach (IDisposable item in items_) {
      item.Dispose();
    }
  }

  private readonly IEnumerable<IDisposable> items_;
}
