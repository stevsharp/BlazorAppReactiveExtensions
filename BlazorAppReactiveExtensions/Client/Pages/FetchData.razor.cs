using BlazorAppReactiveExtensions.Shared;
using Microsoft.AspNetCore.Components;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reactive.Subjects;

namespace BlazorAppReactiveExtensions.Client.Pages
{
    public partial class FetchData : IDisposable
    {

        [Parameter]
        public int Id { get; set; }

        private Subject<int> _ids = new();

        private Subject<bool> _disposed = new();

        private WeatherForecast[]? forecasts;

        private WeatherForecast forecast = new();

        protected override async Task OnInitializedAsync()  
        {
            _ids.DistinctUntilChanged()
                .Select(id => Observable.FromAsync(cancellationToken => LoadWeatherForecastAsync(id, cancellationToken)))
                .Switch()
                .TakeUntil(_disposed)
                .Subscribe(x =>
                {
                    forecast = x;

                    this.StateHasChanged();
                });
        }

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            bool IdSet = parameters.TryGetValue(nameof(Id), out int id);

            await base.SetParametersAsync(parameters);

            if (IdSet)
            {
                _ids.OnNext(id);
            }
        }

        private async Task<WeatherForecast> LoadWeatherForecastAsync(int id, CancellationToken cancellationToken)
        {
            forecasts = await Http.GetFromJsonAsync<WeatherForecast[]>("WeatherForecast", cancellationToken);

            return forecasts.FirstOrDefault(x => x.Id == this.Id);
        }

        public void Dispose()
        {
            _disposed.OnNext(true);
        }
    }
}
