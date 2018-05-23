using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Raven.Client.Documents;
using Raven.Client.Documents.Indexes;

namespace RavenEnumTest
{
	class Program
	{
		static void Main(string[] args)
		{
			var store = new DocumentStore
			{
				Urls = new[] { "http://localhost:8080" },
				Database = "enums",
				Conventions = { SaveEnumsAsIntegers = true }
			};

			store.Initialize();

			new DeviceIndex().Execute(store);

			using (var session = store.OpenSession())
			{
				session.Store(new Device
				{
					Name = "Phone",
					History = new []
					{
						new DeviceHistory
						{
							Timestamp = DateTime.UtcNow.AddHours(-1.0),
							Status = DeviceStatus.Lost
						},
						new DeviceHistory
						{
							Timestamp = DateTime.UtcNow,
							Status = DeviceStatus.Found
						}
					}
				});

				session.SaveChanges();
			}
		}
	}

	class DeviceIndex : AbstractIndexCreationTask<Device>
	{
		public DeviceIndex()
		{
			Map = devices =>
				from device in devices
				let lastHistory = device.History.OrderByDescending(x => x.Timestamp).FirstOrDefault()
				select new
				{
					device.Name,
					Status = lastHistory != null ? (DeviceStatus?) lastHistory.Status : null
				};
		}
	}

	class Device
	{
		public string Name { get; set; }
		public DeviceHistory[] History { get; set; }
	}

	class DeviceHistory
	{
		public DateTime Timestamp { get; set; }
		public DeviceStatus Status { get; set; }
	}

	enum DeviceStatus
	{
		Broken,
		Fixed,
		Lost,
		Found
	}
}
