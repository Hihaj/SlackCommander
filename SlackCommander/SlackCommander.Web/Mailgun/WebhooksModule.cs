﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Web;
using Nancy;
using NLog;
using Refit;
using SlackCommander.Web.Commands;
using TinyMessenger;

namespace SlackCommander.Web.Mailgun
{
    public class WebhooksModule : NancyModule
    {
        private static readonly Logger Log = LogManager.GetCurrentClassLogger();

        public WebhooksModule(
            ITinyMessengerHub hub, 
            IMailgunWebhooks mailgunWebhooks)
        {
            Post["/webhooks/mailgun/{webhookId}/{slackChannel}"] = _ =>
            {
                Log.Debug("Received webhook call from Mailgun.");

                // TODO Verify signature?

                var webhookId = (string)_.webhookId;
                if (webhookId.Missing())
                {
                    Log.Info("Rejected webhook call from Mailgun (WebhookId is missing).");
                    return HttpStatusCode.NotAcceptable.WithReason("WebhookId is missing.");
                }
                var webhook = mailgunWebhooks.Get(webhookId);
                if (webhook == null)
                {
                    Log.Info("Rejected webhook call from Mailgun (webhook '{0}' not found).", webhookId);
                    return HttpStatusCode.NotAcceptable.WithReason("The webhook does not exist.");
                }

                var slackChannel = "#" + ((string)_.slackChannel).TrimStart('#');

                var sender = (string)Request.Form["sender"];
                var recipient = (string)Request.Form["recipient"];
                var subject = (string)Request.Form["subject"];
                var plainBody = (string)Request.Form["body-plain"];

                // HACK: Since Nancy parses subject into "subject,subject", just split in two for now
                subject = subject.Substring(0, subject.Length/2);

                // Send notification to Slack.
                hub.PublishAsync(new TinyMessage<MessageToSlack>(new MessageToSlack
                {
                    channel = slackChannel,
                    text = string.Format("E-mail from *{0}*:\n", sender, recipient),
                    attachments = new[]
                    {
                        new MessageToSlack.Attachment
                        {
                            fallback = subject,
                            text = string.Format("*{0}*\n{1}", subject, plainBody)
                        }
                    }
                }));

                return HttpStatusCode.OK;
            };
        }
    }
}