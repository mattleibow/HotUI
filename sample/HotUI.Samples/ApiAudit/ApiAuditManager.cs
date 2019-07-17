﻿using System;
using System.Collections.Generic;
using HotUI.Reflection;
using HotUI.Internal;
using System.Linq;

namespace HotUI.Samples
{
    public class ApiAuditManager
    {
        static string[] IgnoredProperties =
        {
            nameof(View.Id),
        };

        static string[] FontProperties =
        {
            EnvironmentKeys.Fonts.Font,
            EnvironmentKeys.Colors.Color
        };

        static string[] ViewProperties =
        {
            EnvironmentKeys.Colors.BackgroundColor,
            EnvironmentKeys.View.ClipShape,
            EnvironmentKeys.View.Overlay,
            EnvironmentKeys.View.Shadow,
        };

        static ApiAuditManager()
        {
            Register<View>(ViewProperties);
            Register<Text>(FontProperties);
            Register<Button>(FontProperties);
        }

        static Dictionary<Type, HashSet<string>> WatchedProperties = new Dictionary<Type, HashSet<string>>();
        public static void Register<T>(params string[] properties) where T : View
        {
            var type = typeof(T);
            if (!WatchedProperties.TryGetValue(type, out var propList))
            {
                WatchedProperties[type] = propList = new HashSet<string>();
                var baseProps = type.GetDeepProperties();
                foreach(var prop in baseProps)
                {
                    if(!IgnoredProperties.Contains(prop.Name))
                        propList.Add(prop.Name);
                }
            }

            foreach (var prop in properties)
                propList.Add(prop);
        }

        public static List<AuditReport> GenerateReport()
        {
            var reports = new List<AuditReport>();
            var pairs = Registrar.Handlers.GetAllRenderers();
            foreach(var pair in pairs)
            {
                reports.Add(GenerateReport(pair.Key, pair.Value));
            }
            return reports;
        }

        static AuditReport GenerateReport(Type viewType, Type handler)
        {
            var report = new AuditReport
            {
                View = viewType.Name,
                Handler = handler.FullName,
            };

            if(viewType.BaseType != null)
            {
                var baseHandler = Registrar.Handlers.GetRendererType(viewType);
                if(baseHandler != null)
                {
                    var baseReport = GenerateReport(viewType.BaseType, baseHandler);
                    if(baseReport.HandledProperties?.Count > 0)
                        report.HandledProperties.AddRange(baseReport.HandledProperties);
                    if (baseReport.UnHandledProperties?.Count > 0)
                        report.UnHandledProperties.AddRange(baseReport.UnHandledProperties);
                }
            }


            WatchedProperties.TryGetValue(viewType, out var watchedProps);

            var mapper = handler.GetField("Mapper", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
            if (mapper != null)
            {
                var map = mapper.GetValue(null);
                var mapKeysProp = map.GetType().GetProperty("Keys", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                var keys = (mapKeysProp.GetValue(map) as IEnumerable<string>).ToList();

                if (watchedProps != null)
                {
                    foreach (var prop in watchedProps)
                    {
                        if (keys.Contains(prop))
                        {
                            if (!report.HandledProperties.Contains(prop))
                                report.HandledProperties.Add(prop);
                            if (report.UnHandledProperties.Contains(prop))
                                report.UnHandledProperties.Remove(prop);
                        }
                        else if (!report.HandledProperties.Contains(prop))
                            report.UnHandledProperties.Add(prop);
                    }
                }
                foreach (var key in keys)
                {
                    if (!report.HandledProperties.Contains(key))
                        report.HandledProperties.Contains(key);
                }

            }
            else
            {
                if (watchedProps != null)
                {
                    foreach (var prop in watchedProps)
                    {
                       
                        if (!report.HandledProperties.Contains(prop))
                            report.UnHandledProperties.Add(prop);
                    }
                }
                report.MissingMapper = true;
            }

            return report;

        }

        public class AuditReport
        {
            public string View { get; set; }
            public string Handler { get; set; }
            public List<string> HandledProperties { get; set; } = new List<string>();
            public List<string> UnHandledProperties { get; set; } = new List<string>();
            public bool MissingMapper { get; set; }
            public bool IsComplete => HandledProperties?.Count > 0 && UnHandledProperties?.Count <= 0;
        }
    }
}
