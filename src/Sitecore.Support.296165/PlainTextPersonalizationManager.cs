using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Web;
using Sitecore.Diagnostics;
using Sitecore.Modules.EmailCampaign.Core.Personalization;

namespace Sitecore.Support.Modules.EmailCampaign.Core.Personalization
{
  public class PlainTextPersonalizationManager : Sitecore.Modules.EmailCampaign.Core.Personalization.PersonalizationManager
  {
    private delegate string TokenHandler(string text, string key, IList<TokenMapper> tokenMappers, ref bool moveStartIndex);

    private static readonly TokenHandler ResolveAndModifyTokenHandler = delegate (string text, string key, IList<TokenMapper> tokenMappers, ref bool moveStartIndex)
    {
      TokenValue tokenValue = null;
      if (!string.IsNullOrWhiteSpace(key))
      {
        tokenValue = ResolveAndModifyTokenValue(new Token(key), tokenMappers);
      }

      if (tokenValue != null)
      {
        var value = tokenValue.Value == null ? string.Empty : tokenValue.Value.ToString();
        text = text.Replace("$" + key + "$", value);
      }
      else
      {
        moveStartIndex = true;
      }

      return text;
    };

    private static readonly TokenHandler ModifyTokenHandlerValue = delegate (string text, string key, IList<TokenMapper> tokenMappers, ref bool moveStartIndex)
    {
      IEnumerable<TokenValue> tokenValues = null;
      if (!string.IsNullOrWhiteSpace(key))
      {
        tokenValues = tokenMappers.Select(tokenMapper => tokenMapper.ResolveToken(new Token(key)));
      }

      if (tokenValues != null && tokenValues.Any())
      {
        text = text.Replace("$" + key + "$", key);
      }
      else
      {
        moveStartIndex = true;
      }

      return text;
    };

    private static TokenValue ResolveAndModifyTokenValue([NotNull] Token token, IList<TokenMapper> tokenMappers)
    {
      Assert.ArgumentNotNull(token, "token");

      var value = tokenMappers
        .Select(tokenMapper => tokenMapper.ResolveToken(token))
        .FirstOrDefault(tokenValue => tokenValue.KnownToken);

      return ModifyTokenValue(value, tokenMappers);
    }

    private static TokenValue ModifyTokenValue([CanBeNull] TokenValue tokenValue, IList<TokenMapper> tokenMappers)
    {
      if (tokenValue == null)
      {
        return null;
      }

      if (tokenValue.Value == null || tokenValue.Value.GetType() != typeof(string))
      {
        return tokenValue;
      }

      var text = tokenValue.Value.ToString();

      tokenValue.Value = ModifyText(text, ModifyTokenHandlerValue, tokenMappers);
      return tokenValue;
    }

    private static string ModifyText(string text, TokenHandler handler, IList<TokenMapper> tokenMappers)
    {
      char[] separators =
      {
        ' ',
        '.',
        '\r',
        '\n',
        '<',
        '>'
      };

      var startIndex = text.IndexOf('$');

      while (startIndex > -1 && startIndex < text.Length - 1)
      {
        var endIndex = text.IndexOf('$', startIndex + 1);
        if (endIndex == -1)
        {
          break;
        }

        if (text.IndexOfAny(separators, startIndex + 1, endIndex - startIndex - 1) > -1)
        {
          startIndex = endIndex;
          continue;
        }

        var key = text.Substring(startIndex + 1, endIndex - startIndex - 1);
        var moveStartIndex = false;
        text = handler(text, key, tokenMappers, ref moveStartIndex);

        if (moveStartIndex)
        {
          startIndex = endIndex;
        }

        startIndex = startIndex < text.Length - 1 ? text.IndexOf('$', startIndex) : -1;
      }

      return text;
    }

    public override string ModifyText(string text)
    {
      Assert.ArgumentNotNull(text, "text");

      return ModifyText(text, ResolveAndModifyTokenHandler, GetTokenMappers);
    }

    protected IList<TokenMapper> GetTokenMappers
    {
      get
      {
        return typeof(PersonalizationManager).GetField("_tokenMappers", BindingFlags.Instance | BindingFlags.NonPublic)
          .GetValue(this) as IList<TokenMapper>;
      }
    }

    public PlainTextPersonalizationManager(params TokenMapper[] tokenMappers) : base(tokenMappers)
    {
    }
  }
}