namespace Sitecore.Support.Modules.EmailCampaign.Core
{
  using Sitecore.Data.Items;
  using Sitecore.Modules.EmailCampaign.Core.HostnameMapping;
  using Sitecore.Modules.EmailCampaign.Factories;
  using Sitecore.Modules.EmailCampaign.Messages;
  using Sitecore.Modules.EmailCampaign.Services;

  public class TypeResolver : Sitecore.Modules.EmailCampaign.Core.TypeResolver
  {

    private readonly IMessageItemSourceFactory _messageItemSourceFactory;
    private readonly IManagerRootService _managerRootService;
    public TypeResolver(IHostnameMappingService hostnameMappingService, IMessageItemSourceFactory messageItemSourceFactory, IManagerRootService managerRootService, IMultiVariateTestStrategyFactory multiVariateTestStrategyFactory, IAbnTestService abnTestService)
      : base(hostnameMappingService, messageItemSourceFactory, managerRootService, multiVariateTestStrategyFactory, abnTestService)
    {
      _messageItemSourceFactory = messageItemSourceFactory;
      _managerRootService = managerRootService;
    }

    public override MessageItem GetCorrectMessageObject(Item item)
    {
      if (item != null && TextMail.IsCorrectMessageItem(item))
        return Sitecore.Support.Modules.EmailCampaign.Messages.TextMail.FromItem(item, _messageItemSourceFactory, _managerRootService);
      return base.GetCorrectMessageObject(item);
    }
  }
}