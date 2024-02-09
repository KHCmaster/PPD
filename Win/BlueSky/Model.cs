using Newtonsoft.Json;
using System;
using System.Dynamic;

namespace BlueSky
{
    class CreateSessionRequest
    {
        [JsonProperty("identifier")]
        public string Identifier
        {
            get;
            set;
        }

        [JsonProperty("password")]
        public string Password
        {
            get;
            set;
        }
    }

    class CreateSessionResponse
    {
        [JsonProperty("did")]
        public string Did
        {
            get;
            set;
        }

        [JsonProperty("handle")]
        public string Handle
        {
            get;
            set;
        }

        [JsonProperty("accessJwt")]
        public string AccessJwt
        {
            get;
            set;
        }

        [JsonProperty("refreshJwt")]
        public string RefreshJwt
        {
            get;
            set;
        }
    }

    class RefreshSessionRequest
    {

    }

    class RefreshSessionResponse
    {
        [JsonProperty("did")]
        public string Did
        {
            get;
            set;
        }

        [JsonProperty("handle")]
        public string Handle
        {
            get;
            set;
        }

        [JsonProperty("accessJwt")]
        public string AccessJwt
        {
            get;
            set;
        }

        [JsonProperty("refreshJwt")]
        public string RefreshJwt
        {
            get;
            set;
        }
    }

    class CreatePostRequest
    {
        [JsonProperty("collection")]
        public string Collection
        {
            get;
            set;
        }

        [JsonProperty("repo")]
        public string Repo
        {
            get;
            set;
        }

        [JsonProperty("record")]
        public Record Record
        {
            get;
            set;
        }
    }

    class Record
    {
        [JsonProperty("text")]
        public string Text
        {
            get;
            set;
        }

        [JsonProperty("createdAt")]
        public DateTime CreatedAt
        {
            get;
            set;
        }

        [JsonProperty("$type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("embed")]
        public RecordEmbed Embed
        {
            get;
            set;
        }
    }

    class RecordEmbed
    {
        [JsonProperty("$type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("images")]
        public RecordImage[] Images
        {
            get;
            set;
        }
    }

    class RecordImage
    {
        [JsonProperty("alt")]
        public string Alt
        {
            get;
            set;
        }

        [JsonProperty("image")]
        public Blob Image
        {
            get;
            set;
        }
    }

    class CreatePostResponse
    {
    }

    class UploadBlobResponse
    {
        [JsonProperty("blob")]
        public Blob Blob
        {
            get;
            set;
        }
    }

    class Blob
    {
        [JsonProperty("$type")]
        public string Type
        {
            get;
            set;
        }

        [JsonProperty("ref")]
        public Ref Ref
        {
            get;
            set;
        }

        [JsonProperty("mimeType")]
        public string MimeType
        {
            get;
            set;
        }

        [JsonProperty("size")]
        public int Size
        {
            get;
            set;
        }
    }

    class Ref
    {
        [JsonProperty("$link")]
        public string Link
        {
            get;
            set;
        }
    }

    class RequestError
    {
        [JsonProperty("error")]
        public string Error
        {
            get;
            set;
        }

        [JsonProperty("message")]
        public string Message
        {
            get;
            set;
        }
    }
}
