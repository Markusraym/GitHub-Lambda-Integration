using System.Net.Http;
using System.Text;
using Amazon.Lambda.Core;
using Newtonsoft.Json.Linq;

[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace EsepWebhook
{
    public class Function
    {
        /// <summary>
        /// This function processes GitHub webhook events and sends a notification to Slack.
        /// </summary>
        /// <param name="input">The GitHub event payload.</param>
        /// <param name="context">Lambda context information.</param>
        /// <returns>The response from the Slack API.</returns>
        public string FunctionHandler(JObject input, ILambdaContext context)
        {
            // Extract issue details from the GitHub event
            string issueTitle = input["issue"]?["title"]?.ToString();
            string issueUrl = input["issue"]?["html_url"]?.ToString();
            string issueBody = input["issue"]?["body"]?.ToString();

            // Create a more descriptive message for Slack
            string messageText = $"A new GitHub issue has been created:\n*Title:* {issueTitle}\n*Details:* {issueBody}\n*Link:* {issueUrl}";

            // Construct the JSON payload to send to Slack
            string payload = JsonConvert.SerializeObject(new { text = messageText });

            // Send the message to Slack
            using var client = new HttpClient();
            var slackWebhookUrl = Environment.GetEnvironmentVariable("SLACK_URL");
            var requestContent = new StringContent(payload, Encoding.UTF8, "application/json");
            var response = client.PostAsync(slackWebhookUrl, requestContent).Result;

            // Return the response from Slack
            return response.Content.ReadAsStringAsync().Result;
        }
    }
}
