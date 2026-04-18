using MovieCatalogExam.Models;
using RestSharp;
using RestSharp.Authenticators;
using System.Net;
using System.Text.Json;

namespace MovieCatalog
{
    public class Tests
    {
        //Create comment to test the commit functionality of git
        //Create another comment to test the commit functionality of git
        private RestClient client;
        private static string movieId;

        [OneTimeSetUp]
        public void Setup()
        {
            string jwtToken = GetJwtToken("irina321@abv.bg", "123456");
            RestClientOptions options = new RestClientOptions("http://144.91.123.158:5000")
            {
                Authenticator = new JwtAuthenticator(jwtToken)
            };

            this.client = new RestClient(options);
        }

        private string GetJwtToken(string email, string password)
        {
            var client = new RestClient("http://144.91.123.158:5000");
            var request = new RestRequest("api/User/Authentication", Method.Post);
            request.AddJsonBody(new { email, password });

            var response = client.Execute(request);

            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                var content = JsonSerializer.Deserialize<JsonElement>(response.Content);
                var token = content.GetProperty("accessToken").GetString();

                if (string.IsNullOrEmpty(token))
                {
                    throw new InvalidOperationException("Token is null or empty.");
                }

                return token;
            }

            throw new InvalidOperationException($"Authentication failed with status code: {response.StatusCode}");
        }


        [Order(1)]
        [Test]
        public void CreateNewMovie_WithRequiredFields_ShouldReturnSuccess()
        {
            var newMovie = new
            {
                title = "Test Movie",
                description = "Test Description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };

            var request = new RestRequest("api/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.Movie, Is.Not.Null);
            Assert.That(responseData.Movie.Id, Is.Not.Null.And.Not.Empty);
            Assert.That(responseData.Msg, Is.EqualTo("Movie created successfully!"));

            movieId = responseData.Movie.Id;
        }

        [Order(2)]
        [Test]
        public void Edit_TitleOfCreatedMovie_ShouldReturnSuccess()
        {
            var editedMovie = new
            {
                title = "Updated Test Movie",
                description = "Test Description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };
            var request = new RestRequest("api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", movieId);
            request.AddJsonBody(editedMovie);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);

            Assert.That(responseData.Movie, Is.Not.Null);
            Assert.That(responseData.Msg, Is.EqualTo("Movie edited successfully!"));
        }

        [Order(3)]
        [Test]
        public void GetAllMovies_ShouldListAllMovies()
        {
            var request = new RestRequest("api/Catalog/All", Method.Get);

            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

            var movies = JsonSerializer.Deserialize<List<MovieDto>>(response.Content);

            Assert.That(movies, Is.Not.Null);
            Assert.That(movies.Count, Is.GreaterThan(0));
        }

        [Order(4)]
        [Test]
        public void Delete_CreatedMovie_ShouldReturnSuccess()
        {
            var request = new RestRequest("api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", movieId);
            var response = client.Execute(request);

            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));
            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Movie deleted successfully!"));
        }

        [Order(5)]
        [Test]
        public void CreateMovie_WithoutRequiredFields_ShouldReturnBadRequest()
            {
            var newMovie = new
            {
                description = "Test Description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };
            var request = new RestRequest("api/Movie/Create", Method.Post);
            request.AddJsonBody(newMovie);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
        }

        [Order(6)]
        [Test]
        public void Edit_NonExistentMovie_ShouldReturnBadRequest()
        {
            var editedMovie = new
            {
                title = "Updated Test Movie",
                description = "Test Description",
                posterUrl = "",
                trailerLink = "",
                isWatched = true
            };
            var request = new RestRequest("api/Movie/Edit", Method.Put);
            request.AddQueryParameter("movieId", "non-existent-id");
            request.AddJsonBody(editedMovie);
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));

            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Unable to edit the movie! Check the movieId parameter or user verification!"));
        }

        [Order(7)]
        [Test]
        public void Delete_NonExistentMovie_ShouldReturnBadRequest()
        {
            var request = new RestRequest("api/Movie/Delete", Method.Delete);
            request.AddQueryParameter("movieId", "non-existent-id");
            var response = client.Execute(request);
            Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.BadRequest));
            var responseData = JsonSerializer.Deserialize<ApiResponseDto>(response.Content);
            Assert.That(responseData.Msg, Is.EqualTo("Unable to delete the movie! Check the movieId parameter or user verification!"));
        }

        [OneTimeTearDown]
        public void TearDown()
        {
            this.client?.Dispose();

        }
    }
}