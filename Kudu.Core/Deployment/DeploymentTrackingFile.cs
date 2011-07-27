﻿using System;
using System.IO;
using System.Xml.Linq;

namespace Kudu.Core.Deployment {
    /// <summary>
    /// An xml file that keeps track of deployment status
    /// </summary>
    public class DeploymentTrackingFile {
        private readonly string _path;

        private DeploymentTrackingFile(string path) {
            _path = path;
        }

        public static DeploymentTrackingFile Create(string path) {
            return new DeploymentTrackingFile(path) {
                DeployTime = DateTime.UtcNow
            };
        }

        public static DeploymentTrackingFile Open(string path) {
            XDocument document;

            try {
                using (var stream = File.OpenRead(path)) {
                    document = XDocument.Load(stream);
                }
            }
            catch {
                return null;
            }

            int percentage;
            Int32.TryParse(document.Root.Element("percentage").Value, out percentage);

            DeployStatus status;
            Enum.TryParse(document.Root.Element("status").Value, out status);

            return new DeploymentTrackingFile(path) {
                Id = document.Root.Element("id").Value,
                Status = status,
                StatusText = document.Root.Element("statusText").Value,
                Percentage = percentage,
                DeployTime = DateTime.Parse(document.Root.Element("deployTime").Value)
            };
        }

        public string Id { get; set; }
        public DeployStatus Status { get; set; }
        public string StatusText { get; set; }
        public int Percentage { get; set; }
        public DateTime DeployTime { get; private set; }

        public void Save() {
            if (String.IsNullOrEmpty(Id)) {
                throw new InvalidOperationException();
            }

            new XDocument(new XElement("deployment",
                    new XElement("id", Id),
                    new XElement("status", Status),
                    new XElement("statusText", StatusText),
                    new XElement("percentage", Percentage),
                    new XElement("deployTime", DeployTime)
                )).Save(_path);
        }
    }
}
