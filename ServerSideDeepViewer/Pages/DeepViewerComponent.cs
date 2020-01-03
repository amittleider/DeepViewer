using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using VSLee.IEXSharp;
using VSLee.IEXSharp.Helper;
using VSLee.IEXSharp.Model.Shared.Response;

namespace ServerSideDeepViewer
{
    public class DeepViewerComponent : ComponentBase
    {
        internal List<string> Messages { get; set; } = new List<string>();
        internal string SecretKey;
        internal string PublishableKey;
        internal string StockSymbol;
        private IEXCloudClient sandBoxClient;
        private SSEClient<QuoteCrypto> sseClient;

        protected override async Task OnInitializedAsync()
        {
            this.Messages.Add("Hello");
        }

        private void OnNewMessage(string message)
        {
            this.Messages.Add(message);
        }

        private async Task ComponentMessageReceived()
        {
            this.Messages.Add("This message doesn't appear");
            await this.InvokeAsync(StateHasChanged);
        }

        internal async Task Subscribe()
        {
            await Task.Run(async () =>
                {
                    for (int i = 0; i < 3; i++)
                    {
                        await Task.Delay(500);
                        await ComponentMessageReceived();
                    }
                }
            );

            new Thread(async () =>
                {
                    Thread.CurrentThread.IsBackground = true;
                    this.sandBoxClient = new IEXCloudClient(publishableToken: "Tpk_0bc6fda638b84be5a2991fc334ce516c", secretToken: "Tsk_2761d3806c9c4bd6aa1ee70fc981a430", signRequest: false, useSandBox: true);
                    this.sseClient = sandBoxClient.SSE.SubscribeCryptoQuoteSSE(new List<string>() { "btcusdt" });

                    this.OnNewMessage("Starting");
                    sseClient.MessageReceived += (e) => MessageReceived(e);
                    await sseClient.StartAsync();
                    this.OnNewMessage("Started");
                }
            ).Start();

            this.OnNewMessage("Done");
        }

        private void MessageReceived(List<QuoteCrypto> e)
        {
            foreach (var m in e)
            {
                this.Messages.Add(m.ToString());
                this.InvokeAsync(StateHasChanged);
            }
        }
    }
}
