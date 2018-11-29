namespace Sitecore.Support.Modules.EmailCampaign.Messages
{
  using Sitecore.Data.Items;
  using Sitecore.Modules.EmailCampaign.Core.Personalization;
  using Sitecore.Modules.EmailCampaign.Factories;
  using Sitecore.Modules.EmailCampaign.Services;
  using Sitecore.EmailCampaign.Model.Message;
  using Sitecore.Support.Modules.EmailCampaign.Core.Personalization;

  public class TextMail : Sitecore.Modules.EmailCampaign.Messages.TextMail
  {
    private PersonalizationManager _personalizationManager;

    protected TextMail(Item item, IMessageItemSourceFactory messageItemSourceFactory, IManagerRootService managerRootService) : base(item, messageItemSourceFactory, managerRootService)
    {
    }
    public new static TextMail FromItem(Item item, IMessageItemSourceFactory messageItemSourceFactory, IManagerRootService managerRootService)
    {
      if (!IsCorrectMessageItem(item))
      {
        return null;
      }
      return new TextMail(item, messageItemSourceFactory, managerRootService);
    }

    public override object Clone()
    {
      TextMail newMessage = new TextMail(InnerItem, MessageItemSourceFactory, ManagerRootService);
      this.CloneFields(newMessage);
      return newMessage;
    }

    protected override PersonalizationManager PersonalizationManager
    {
      get
      {
        if (_personalizationManager == null)
        {
          _personalizationManager = new PlainTextPersonalizationManager();
          if (CustomPersonTokens.Count > 0)
          {
            var mapper = new DictionaryTokenMapper();

            foreach (var token in CustomPersonTokens)
            {
              if (!string.IsNullOrWhiteSpace(token.Key) && token.Value != null)
              {
                mapper.BindToken(new Token(token.Key), token.Value);
              }
            }

            _personalizationManager.AddTokenMapper(mapper);
          }
          if (PersonalizationRecipient != null)
          {
            var mapper = new RecipientPropertyTokenMapper(PersonalizationRecipient);
            _personalizationManager.AddTokenMapper(mapper);
          }
          else if (MessageType == MessageType.Automated && Context.User != null)
          {
            // Assume we are in the context of a "Quick test" or Message tab preview
            var mapper = new ContextUserTokenMapper();
            _personalizationManager.AddTokenMapper(mapper);
          }
        }
        return _personalizationManager;
      }
      set { _personalizationManager = value; }
    }
  }
}