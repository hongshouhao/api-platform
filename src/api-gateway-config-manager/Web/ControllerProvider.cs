using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.AspNetCore.Mvc.Controllers;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ApiGatewayManager.Web
{
    public class ControllerProvider : ControllerFeatureProvider, IApplicationFeatureProvider<ControllerFeature>
    {
        static Assembly assembly = typeof(ControllerProvider).Assembly;
        static ControllerFeatureProvider provider = new ControllerFeatureProvider();

        public new void PopulateFeature(IEnumerable<ApplicationPart> parts, ControllerFeature feature)
        {
            TypeInfo[] cinfos = feature.Controllers.ToArray();
            feature.Controllers.Clear();
            foreach (var item in cinfos)
            {
                if (IsController(item))
                {
                    feature.Controllers.Add(item);
                }
            }
        }

        protected override bool IsController(TypeInfo typeInfo)
        {
            return base.IsController(typeInfo) && typeInfo.Assembly.FullName == assembly.FullName;
        }
    }
}
