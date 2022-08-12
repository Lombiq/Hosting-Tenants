using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lombiq.Hosting.MultiTenancy.Tenants.Models;
public class AlwaysOnFeaturesOptions
{
    public IEnumerable<string> AlwaysOnFeatures { get; set; }
}
