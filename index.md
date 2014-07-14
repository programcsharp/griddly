---
layout: page
title: Griddly
tagline: MVC/AJAX/HTML5 hybrid grid
---
{% include JB/setup %}

After researching all of the ASP.NET MVC grid components like [WebGrid](http://msdn.microsoft.com/en-us/library/system.web.helpers.webgrid(v=vs.111).aspx) and the Javascript grid components like [DataTables](http://www.datatables.net/), I found them all lacking -- either the grid is rendered on the server and is kludgy or its rendered client only giving an extra request on page load. Data and render logic is tightly coupled, etc.

Griddly solves those problems by separating render and data logic, rendering in page during the first render, then requesting subsequent pages from the same MVC action method using a hybrid approach.

Griddly is built to be extremely configurable, end to end, from data source to rendering. You can use the default IQueryable data source or write your own. There are default template hooks for most things you would want to change. If not, let us know and we'll add what you need.

The default UI is rendered using [Bootstrap 3](http://getbootstrap.com), but you can add templates for whatever UI framework you use.

### Example

Model:

{% highlight C# %}
public class Person
{
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string ZipCode { get; set; }
}
{% endhighlight %}