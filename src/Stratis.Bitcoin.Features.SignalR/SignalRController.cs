﻿using Microsoft.AspNetCore.Mvc;

namespace Stratis.Bitcoin.Features.SignalR
{
    /// <summary>
    /// Provides methods to support SignalR clients of the full node.
    /// </summary>
    [Route("api/[controller]")]
    public class SignalRController : Controller
    {
        private readonly ISignalRService signalRService;

        public SignalRController(ISignalRService signalRService) => this.signalRService = signalRService;

        /// <summary>Address used by clients when establishing a connection to the fullnode's SignalR hub.</summary>
        [HttpGet]
        [Route("address")]
        public IActionResult Address() => this.Content(this.signalRService.HubRoute.AbsoluteUri);
    }
}
