Bugs / enhancements
-------------------

If you find a bug in the tool or want a new feature implemented, feel more than welcome
to create an [issue](https://github.com/fernandreu/office-ribbonx-editor/issues) and I will have a look at it.


Pull requests
-------------

Pull requests are welcome, but it is always better if there is no duplication of work:

- If you are working on a bug / enhancement that is already listed as an issue, please
  leave a comment saying that you intent to do so. I can then share my thoughts about
  how to address that issue, assign you to it, etc.
- If there is no issue for it, it probably means I am not actively working on that,
  hence you should be good to go. However, it is preferable if an issue is created
  beforehand, in case I have some reservations about it


Coding guidelines
-----------------

The following list is a high-level overview of the guidelines I have followed when
developing the tool. Please adhere to these as much as possible when creating a pull
request:

- Follow the [StyleCop](https://github.com/StyleCop/StyleCop) rules to ensure the
  code formatting / structure / etc. is consistent. There are a few exceptions to 
  this, such as:
    - Order of private / public class members and types: just put them in the order
	  that might make navigation easier. For example, I usually prefer having a 
	  private field defined right above its associated public property.
	- Documentation for every file / class / member: there is no strong reason to
	  increase the size of the source code files if the comments are not going
	  to be meaningful and / or the element might be self-explanatory.
	- Naming convention: recently, I have switched to naming private fields with a
	  leading underscore, such as `_privateField`. As a consequence, I have decided
	  to no longer use `this.` everywhere, since the distinction between a local
	  variable and a private field is now clearer.
- Follow the MVVM pattern, clearly separating the view from the view model.
- Use dependency injection when actions need to be mocked during unit test (e.g. 
  displaying a message box or a file dialog).
- If you need to call a view method from its view model, declare an event in the
  view model and listen to it from the view.
- To ensure code completion / refactoring actions work in XAML files too, specify
  `d:DataContext` attributes in those cases where it is not automatically inferred.
- It is always better to have at least one unit test that ensures that a bug is
  fixed or a new feature works as intended.
