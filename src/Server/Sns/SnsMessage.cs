using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    /// <summary>
    /// An SNS message record.
    /// </summary>
    public class SnsMessage<T> where T : notnull
    {
        /// <summary>
        /// The message.
        /// </summary>
        [JsonConverter(typeof(SnsMessageConverterFactory))]
        public T? Message { get; init; }

        /// <summary>
        /// The attributes associated with the message.
        /// </summary>
        public IDictionary<string, MessageAttribute> MessageAttributes { get; init; }

        /// <summary>
        /// The message id.
        /// </summary>
        public string MessageId { get; init; }

        /// <summary>
        /// The message signature.
        /// </summary>
        public string Signature { get; init; }

        /// <summary>
        /// The signature version used to sign the message.
        /// </summary>
        public string SignatureVersion { get; init; }

        /// <summary>
        /// The URL for the signing certificate.
        /// </summary>
        public Uri SigningCertUrl { get; init; }

        /// <summary>
        /// The subject for the message.
        /// </summary>
        public string Subject { get; init; }

        /// <summary>
        /// The message time stamp.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// The topic ARN.
        /// </summary>
        public string TopicArn { get; init; }

        /// <summary>
        /// The message type.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// The message subscribe URL.
        /// </summary>
        public Uri SubscribeUrl { get; init; }

        /// <summary>
        /// The message unsubscribe URL.
        /// </summary>
        public Uri UnsubscribeUrl { get; init; }
    }
}
