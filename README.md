CSV Content Widget
----------------------------------------

### What is it?
CSV Content Widget is a Sitefinity MVC Widget which takes a CSV file and provides a strongly typed representation of the file for rendering.

One of the typical scenarios of how to use this widget would be a feature list of a software. Let's take a following example:

xamarin.csv

> ,Free, Indie, Business, Entreprise

> Permitted use, Individual, Individual, Organization, Organization

> Deploy to device, x, x, x, x

> Unlimited App Size, , x, x, x

Once we load this through a CsvContent widget, we can visualize this in our view as follows:

```html 
@model SitefinityWebApp.Mvc.Models.CsvContentModel

@if(string.IsNullOrEmpty(Model.ErrorMessage)) {

<table>
    @foreach (var row in Model.Table.Rows)
    {
        <tr class="@(row.IsHeader ? "header" : "justline")">
            @foreach (var col in row.Columns)
            {
                <td>
                    <span class="@(col.IsMarked ? "marked" : "empty")">
                        @col.Value
                    </span>
                </td>
            }
        </tr>
    }
</table>
    
} else {
    <strong>@Model.ErrorMessage</strong>   
}
```

### How to load a file?

There are two ways to load the file in the widget:
1. When you drag widget on the page, click edit and set the CsvFileUrl property to the url of the csv file. If the file is located in the same site as the widget, you can use tilde (~) url. E.g. ~/docs/mysoftwarefeatures.csv
2. In your controller, when initializing the model, you can use the constructor with `string fileContents` argument and provide the string representation of the CSV file
