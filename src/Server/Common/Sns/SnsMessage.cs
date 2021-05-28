using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Brighid.Identity.Sns
{
    /// <summary>
    /// An SNS message record.
    /// </summary>
    /// <typeparam name="T">Type of the message.</typeparam>
    public class SnsMessage<T>
        where T : notnull
    {
        /// <summary>
        /// Gets the message.
        /// </summary>
        [JsonConverter(typeof(SnsMessageConverterFactory))]
        public T? Message { get; init; }

        /// <summary>
        /// Gets the attributes associated with the message.
        /// </summary>
        public IDictionary<string, SnsMessageAttribute> MessageAttributes { get; init; }

        /// <summary>
        /// Gets the message id.
        /// </summary>
        public string MessageId { get; init; }

        /// <summary>
        /// Gets the message signature.
        /// </summary>
        public string Signature { get; init; }

        /// <summary>
        /// Gets the signature version used to sign the message.
        /// </summary>
        public string SignatureVersion { get; init; }

        /// <summary>
        /// Gets the URL for the signing certificate.
        /// </summary>
        public Uri SigningCertUrl { get; init; }

        /// <summary>
        /// Gets the subject for the message.
        /// </summary>
        public string Subject { get; init; }

        /// <summary>
        /// Gets the message time stamp.
        /// </summary>
        public DateTime Timestamp { get; init; }

        /// <summary>
        /// Gets the topic ARN.
        /// </summary>
        public string TopicArn { get; init; }

        /// <summary>
        /// Gets the message type.
        /// </summary>
        public string Type { get; init; }

        /// <summary>
        /// Gets the message subscribe URL.
        /// </summary>
        public Uri SubscribeURL { get; init; }

        /// <summary>
        /// Gets the message unsubscribe URL.
        /// </summary>
        public Uri UnsubscribeURL { get; init; }
    }
}
