﻿using Microsoft.Extensions.Configuration;
using Ocelot.DependencyInjection;
using Shouldly;
using TestStack.BDDfy;
using Xunit;

namespace Ocelot.UnitTests.DependencyInjection
{
    using System.Collections.Generic;
    using System.IO;
    using Newtonsoft.Json;
    using Ocelot.Configuration.File;

    public class ConfigurationBuilderExtensionsTests
    {
        private IConfigurationRoot _configuration;
        private string _result;
        private IConfigurationRoot _configRoot;
        private FileConfiguration _globalConfig;
        private FileConfiguration _reRoute;
        private FileConfiguration _reRouteB;
        private FileConfiguration _aggregate;

        [Fact]
        public void should_add_base_url_to_config()
        {
            this.Given(_ => GivenTheBaseUrl("test"))
                .When(_ => WhenIGet("BaseUrl"))
                .Then(_ => ThenTheResultIs("test"))
                .BDDfy();
        }

        [Fact]
        public void should_merge_files()
        {
            this.Given(_ => GivenMultipleConfigurationFiles())
                .When(_ => WhenIAddOcelotConfiguration())
                .Then(_ => ThenTheConfigsAreMerged())
                .BDDfy();
        }

        private void GivenMultipleConfigurationFiles()
        {
            _globalConfig = new FileConfiguration
            {
                GlobalConfiguration = new FileGlobalConfiguration
                {
                    BaseUrl = "BaseUrl",
                    RateLimitOptions = new FileRateLimitOptions
                    {
                        HttpStatusCode = 500,
                        ClientIdHeader = "ClientIdHeader",
                        DisableRateLimitHeaders = true,
                        QuotaExceededMessage = "QuotaExceededMessage",
                        RateLimitCounterPrefix = "RateLimitCounterPrefix"
                    },
                    ServiceDiscoveryProvider = new FileServiceDiscoveryProvider
                    {
                        Host = "Host",
                        Port = 80,
                        Type = "Type"
                    },
                    RequestIdKey = "RequestIdKey"
                }
            };

            _reRoute = new FileConfiguration
            {
                ReRoutes = new List<FileReRoute>
                {
                    new FileReRoute
                    {
                        DownstreamScheme = "DownstreamScheme",
                        DownstreamPathTemplate = "DownstreamPathTemplate",
                        Key = "Key",
                        UpstreamHost = "UpstreamHost",
                        UpstreamHttpMethod = new List<string>
                        {
                            "UpstreamHttpMethod"
                        },
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "Host",
                                Port = 80
                            }
                        }
                    }
                }
            };

            _reRouteB = new FileConfiguration
            {
                ReRoutes = new List<FileReRoute>
                {
                    new FileReRoute
                    {
                        DownstreamScheme = "DownstreamSchemeB",
                        DownstreamPathTemplate = "DownstreamPathTemplateB",
                        Key = "KeyB",
                        UpstreamHost = "UpstreamHostB",
                        UpstreamHttpMethod = new List<string>
                        {
                            "UpstreamHttpMethodB"
                        },
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "HostB",
                                Port = 80
                            }
                        }
                    },
                    new FileReRoute
                    {
                        DownstreamScheme = "DownstreamSchemeBB",
                        DownstreamPathTemplate = "DownstreamPathTemplateBB",
                        Key = "KeyBB",
                        UpstreamHost = "UpstreamHostBB",
                        UpstreamHttpMethod = new List<string>
                        {
                            "UpstreamHttpMethodBB"
                        },
                        DownstreamHostAndPorts = new List<FileHostAndPort>
                        {
                            new FileHostAndPort
                            {
                                Host = "HostBB",
                                Port = 80
                            }
                        }
                    }
                }
            };

            _aggregate = new FileConfiguration
            {
                Aggregates = new List<FileAggregateReRoute>
                {
                    new FileAggregateReRoute
                    {
                        ReRouteKeys = new List<string> 
                        {
                            "KeyB",
                            "KeyBB"
                        },
                        UpstreamPathTemplate = "UpstreamPathTemplate",
                    },
                    new FileAggregateReRoute
                    {
                        ReRouteKeys = new List<string> 
                        {
                            "KeyB",
                            "KeyBB"
                        },
                        UpstreamPathTemplate = "UpstreamPathTemplate",
                    }
                }
            };

            var globalJson = JsonConvert.SerializeObject(_globalConfig);
            File.WriteAllText("ocelot.global.json", globalJson);

            var reRouteJson = JsonConvert.SerializeObject(_reRoute);
            File.WriteAllText("ocelot.reRoutes.json", reRouteJson);

            var reRouteJsonB = JsonConvert.SerializeObject(_reRouteB);
            File.WriteAllText("ocelot.reRoutesB.json", reRouteJsonB);

            var aggregates = JsonConvert.SerializeObject(_aggregate);
            File.WriteAllText("ocelot.aggregates.json", aggregates);
        }

        private void WhenIAddOcelotConfiguration()
        {
            IConfigurationBuilder builder = new ConfigurationBuilder();
            builder.AddOcelot();
            _configRoot = builder.Build();
        }

        private void ThenTheConfigsAreMerged()
        {
            var fc = (FileConfiguration)_configRoot.Get(typeof(FileConfiguration));

            fc.GlobalConfiguration.BaseUrl.ShouldBe(_globalConfig.GlobalConfiguration.BaseUrl);
            fc.GlobalConfiguration.RateLimitOptions.ClientIdHeader.ShouldBe(_globalConfig.GlobalConfiguration.RateLimitOptions.ClientIdHeader);
            fc.GlobalConfiguration.RateLimitOptions.DisableRateLimitHeaders.ShouldBe(_globalConfig.GlobalConfiguration.RateLimitOptions.DisableRateLimitHeaders);
            fc.GlobalConfiguration.RateLimitOptions.HttpStatusCode.ShouldBe(_globalConfig.GlobalConfiguration.RateLimitOptions.HttpStatusCode);
            fc.GlobalConfiguration.RateLimitOptions.QuotaExceededMessage.ShouldBe(_globalConfig.GlobalConfiguration.RateLimitOptions.QuotaExceededMessage);
            fc.GlobalConfiguration.RateLimitOptions.RateLimitCounterPrefix.ShouldBe(_globalConfig.GlobalConfiguration.RateLimitOptions.RateLimitCounterPrefix);
            fc.GlobalConfiguration.RequestIdKey.ShouldBe(_globalConfig.GlobalConfiguration.RequestIdKey);
            fc.GlobalConfiguration.ServiceDiscoveryProvider.Host.ShouldBe(_globalConfig.GlobalConfiguration.ServiceDiscoveryProvider.Host);
            fc.GlobalConfiguration.ServiceDiscoveryProvider.Port.ShouldBe(_globalConfig.GlobalConfiguration.ServiceDiscoveryProvider.Port);
            fc.GlobalConfiguration.ServiceDiscoveryProvider.Type.ShouldBe(_globalConfig.GlobalConfiguration.ServiceDiscoveryProvider.Type);

            fc.ReRoutes.Count.ShouldBe(_reRoute.ReRoutes.Count + _reRouteB.ReRoutes.Count);

            fc.ReRoutes.ShouldContain(x => x.DownstreamPathTemplate == _reRoute.ReRoutes[0].DownstreamPathTemplate);
            fc.ReRoutes.ShouldContain(x => x.DownstreamPathTemplate == _reRouteB.ReRoutes[0].DownstreamPathTemplate);
            fc.ReRoutes.ShouldContain(x => x.DownstreamPathTemplate == _reRouteB.ReRoutes[1].DownstreamPathTemplate);

            fc.ReRoutes.ShouldContain(x => x.DownstreamScheme == _reRoute.ReRoutes[0].DownstreamScheme);
            fc.ReRoutes.ShouldContain(x => x.DownstreamScheme == _reRouteB.ReRoutes[0].DownstreamScheme);
            fc.ReRoutes.ShouldContain(x => x.DownstreamScheme == _reRouteB.ReRoutes[1].DownstreamScheme);

            fc.ReRoutes.ShouldContain(x => x.Key == _reRoute.ReRoutes[0].Key);
            fc.ReRoutes.ShouldContain(x => x.Key == _reRouteB.ReRoutes[0].Key);
            fc.ReRoutes.ShouldContain(x => x.Key == _reRouteB.ReRoutes[1].Key);

            fc.ReRoutes.ShouldContain(x => x.UpstreamHost == _reRoute.ReRoutes[0].UpstreamHost);
            fc.ReRoutes.ShouldContain(x => x.UpstreamHost == _reRouteB.ReRoutes[0].UpstreamHost);
            fc.ReRoutes.ShouldContain(x => x.UpstreamHost == _reRouteB.ReRoutes[1].UpstreamHost);

            fc.Aggregates.Count.ShouldBe(_aggregate.Aggregates.Count);
        }

        private void GivenTheBaseUrl(string baseUrl)
        {
            #pragma warning disable CS0618
            var builder = new ConfigurationBuilder()
                .AddOcelotBaseUrl(baseUrl);
            #pragma warning restore CS0618
            _configuration = builder.Build();
        }

        private void WhenIGet(string key)
        {
            _result = _configuration.GetValue("BaseUrl", "");
        }

        private void ThenTheResultIs(string expected)
        {
            _result.ShouldBe(expected);
        }
    }
}
