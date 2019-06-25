using System;

namespace ApiGatewayManager.Ids4Conf
{
    public class IdentityAuthOptionsConfig
    {
        public Guid Id { get; set; }
        public string JsonString { get; set; }
        public string Description { get; set; }
        public DateTime? CreateTime { get; set; }
        public DateTime? ModifiedTime { get; set; }
    }
}
