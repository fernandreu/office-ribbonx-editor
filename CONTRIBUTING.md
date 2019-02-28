Bugs / enhancements
-------------------

If you find a bug in the tool or want a new feature implemented, please create an
[issue](/issues) and I will have a look at it.


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
  code formatting / structure / etc. is consistent.
- Follow the MVVM pattern, clearly separating the view from the viewmodel (I have not
  been particularly good at this with the smaller views, but it is in my TODO list).
- Use dependency injection when actions need to be mocked during unit test (e.g. 
  displaying a message box or a file dialog).
- If you need to call a view method from its viewmodel, declare an event in the
  viewmodel and listen to it from the view.
- To ensure code completion / refactoring actions work in XAML files too, specify
  `d:DataContext` attributes in those cases where it is not automatically inferred.
- It is always better to have at least one unit test that ensures that a bug is
  fixed or a new feature works as intended.
- All resources should be embedded in the executable (I want the tool to be able to
  run from anywhere without installation or a special folder structure).
