using System.Collections.Generic;
using System.Linq;
using ApiServer.Controllers;
using ApiServer.Models;
using Xunit;

namespace ApiServer.Tests
{
    public class WeatherForecastTests
    {
        [Fact]
        public void Get_ReturnsWeatherForecasts()
        {
            // Arrange
            var controller = new WeatherForecastController();

            // Act
            var result = controller.Get();

            // Assert
            var forecasts = Assert.IsType<WeatherForecast[]>(result);
            Assert.Equal(5, forecasts.Length);
        }
    }
}
