using System.Globalization;
using Microsoft.AspNetCore.Mvc.ModelBinding;
namespace Taller.Web.Models;
// HTML input[type=number] sends a dot regardless of the display culture.
public class DecimalModelBinder:IModelBinder {
 public Task BindModelAsync(ModelBindingContext context) {
  var value=context.ValueProvider.GetValue(context.ModelName);
  if(value==ValueProviderResult.None)return Task.CompletedTask;
  context.ModelState.SetModelValue(context.ModelName,value);
  if(decimal.TryParse(value.FirstValue,NumberStyles.AllowLeadingSign|NumberStyles.AllowDecimalPoint,CultureInfo.InvariantCulture,out var result))context.Result=ModelBindingResult.Success(result);
  else context.ModelState.TryAddModelError(context.ModelName,"Ingrese un importe válido, con punto decimal y sin separador de miles.");
  return Task.CompletedTask;
 }
}
public class DecimalModelBinderProvider:IModelBinderProvider {
 public IModelBinder? GetBinder(ModelBinderProviderContext context)=>context.Metadata.ModelType==typeof(decimal)?new DecimalModelBinder():null;
}
