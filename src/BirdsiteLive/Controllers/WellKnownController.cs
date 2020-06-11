﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BirdsiteLive.Common.Settings;
using BirdsiteLive.Models;
using BirdsiteLive.Twitter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace BirdsiteLive.Controllers
{
    [ApiController]
    public class WellKnownController : ControllerBase
    {
        private readonly ITwitterService _twitterService;
        private readonly InstanceSettings _settings;

        #region Ctor
        public WellKnownController(IOptions<InstanceSettings> settings, ITwitterService twitterService)
        {
            _twitterService = twitterService;
            _settings = settings.Value;
        }
        #endregion

        [Route("/.well-known/webfinger")]
        public IActionResult Webfinger(string resource = null)
        {
            var acct = resource.Split("acct:")[1].Trim();

            string name = null;
            string domain = null;

            var splitAcct = acct.Split('@', StringSplitOptions.RemoveEmptyEntries);

            var atCount = acct.Count(x => x == '@');
            if (atCount == 1 && acct.StartsWith('@'))
            {
                name = splitAcct[1];
            }
            else if (atCount == 1 || atCount == 2)
            {
                name = splitAcct[0];
                domain = splitAcct[1];
            }
            else
            {
                return BadRequest();
            }

            if (!string.IsNullOrWhiteSpace(domain) && domain != _settings.Domain)
                return NotFound();
            
            var user = _twitterService.GetUser(name);
            if (user == null)
                return NotFound();

            var result = new WebFingerResult()
            {
                subject = $"acct:{name}@{_settings.Domain}",
                aliases = new []
                {
                    $"https://{_settings.Domain}/@{name}",
                    $"https://{_settings.Domain}/users/{name}"
                },
                links = new List<WebFingerLink>
                {
                    new WebFingerLink()
                    {
                        rel = "http://webfinger.net/rel/profile-page",
                        type = "text/html",
                        href = $"https://{_settings.Domain}/@{name}"
                    },
                    new WebFingerLink()
                    {
                        rel = "self",
                        type = "application/activity+json",
                        href = $"https://{_settings.Domain}/users/{name}"
                    }
                }
            };

            return new JsonResult(result);
        }

        public class WebFingerResult
        {
            public string subject { get; set; }
            public string[] aliases { get; set; }
            public List<WebFingerLink> links { get; set; } = new List<WebFingerLink>();
        }

        public class WebFingerLink
        {
            public string rel { get; set; }
            public string type { get; set; }
            public string href { get; set; }
        }
    }
}