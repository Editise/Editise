This tiny CMS has been created as an easy way to build microsites and brochure websites

You can see and example of it in action here

https://adapptesters.com

There the content is powered by Word DOCX files in the docs folder (DOCX files can be created with MS Word or OpenOffice) . The server code is .NET 6 cross platform and works on Linux, Windows and Mac

More doumentation and information will follow as this project evolves

Inserting text is done by using the Docusert HTML Component. This references a Word Document and looks for the text in a block with matching H1 (Header 1) Title e.g.

<div class="col-sm-12 col-md-6">
    <h3><Docusert DocSelect="Page3.docx" BlockSelect="Block1Title"></Docusert></h3>
    <p><Docusert DocSelect="Page3.docx" BlockSelect="Block1Text"></Docusert></p>
</div>

To run the the project simply clone / download and then open in Visual Studio 2022.

Resource requirements are very light, and this should run on the smallest of serverless instances without any issues. All data is held in memory, making it lightning fast.

If you would like to see further development of this tiny CMS then let us know by starring the project.

This project is currently sponsored by https://adappt.ai and we are actively looking for more sponsors to abe able to add more free features 

This project is Open Source under MIT
