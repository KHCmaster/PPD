using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BlueSky
{
    public class Client
    {
        private readonly string id;
        private readonly string password;
        private readonly HttpClient client;
        private readonly JsonSerializerSettings jsonSerializerSettings = new JsonSerializerSettings()
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
        };
        private bool isLogined;
        private string did;
        private string accessJwt;
        private string refreshJwt;
        private Jwt jwt;

        public Client(string id, string password)
        {
            this.id = id;
            this.password = password;
            this.client = new HttpClient();
        }

        public async Task Post(string message)
        {
            await EnsureLogin();
            await this.PostAsync<CreatePostRequest, CreatePostResponse>("com.atproto.repo.createRecord", new CreatePostRequest
            {
                Collection = "app.bsky.feed.post",
                Repo = this.did,
                Record = new Record
                {
                    Text = message,
                    CreatedAt = DateTime.UtcNow,
                    Type = "app.bsky.feed.post",
                },
            });
        }

        public async Task PostImage(string message, byte[] image, string contentType)
        {
            await EnsureLogin();
            var content = new ByteArrayContent(image);
            content.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(contentType);
            var response = await this.client.PostAsync("https://bsky.social/xrpc/com.atproto.repo.uploadBlob", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(JsonConvert.DeserializeObject<RequestError>(responseString).Message);
            }
            var uploadResponse = JsonConvert.DeserializeObject<UploadBlobResponse>(responseString, jsonSerializerSettings);
            await this.PostAsync<CreatePostRequest, CreatePostResponse>("com.atproto.repo.createRecord", new CreatePostRequest
            {
                Collection = "app.bsky.feed.post",
                Repo = this.did,
                Record = new Record
                {
                    Text = message,
                    CreatedAt = DateTime.UtcNow,
                    Type = "app.bsky.feed.post",
                    Embed = new RecordEmbed
                    {
                        Type = "app.bsky.embed.images",
                        Images = new RecordImage[]
                        {
                            new RecordImage
                            {
                                Alt = "",
                                Image = uploadResponse.Blob,
                            }
                        }
                    },
                },
            });
        }

        private async Task EnsureLogin()
        {
            if (isLogined)
            {
                if (DateTime.UtcNow.AddMinutes(5) > this.jwt.Exp)
                {
                    await this.Refresh();
                }
                return;
            }
            var response = await this.PostAsync<CreateSessionRequest, CreateSessionResponse>("com.atproto.server.createSession", new CreateSessionRequest
            {
                Identifier = this.id,
                Password = this.password,
            });
            this.did = response.Did;
            this.accessJwt = response.AccessJwt;
            this.refreshJwt = response.RefreshJwt;
            this.jwt = Jwt.Parse(this.accessJwt);
            this.isLogined = true;
            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.accessJwt);
        }

        private async Task Refresh()
        {
            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.refreshJwt);
            var response = await this.PostAsync<RefreshSessionRequest, RefreshSessionResponse>("com.atproto.server.refreshSession", new RefreshSessionRequest { });
            this.did = response.Did;
            this.accessJwt = response.AccessJwt;
            this.refreshJwt = response.RefreshJwt;
            this.client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", this.accessJwt);
        }

        private async Task<TResponse> PostAsync<TRequest, TResponse>(string rpc, TRequest request)
        {
            var content = new JsonContent<TRequest>(request, jsonSerializerSettings);
            var response = await this.client.PostAsync($"https://bsky.social/xrpc/{rpc}", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (response.StatusCode != System.Net.HttpStatusCode.OK)
            {
                throw new Exception(JsonConvert.DeserializeObject<RequestError>(responseString, jsonSerializerSettings).Message);
            }
            return JsonConvert.DeserializeObject<TResponse>(responseString, jsonSerializerSettings);
        }
    }
}
