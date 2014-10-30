Snapsy
======

A simple scan-to-PDF application for Windows only, supporting TWAIN and WIA for image acquisition.

*Project status: unmaintained* (see below)

More info
---------

This is a fork of [Not Another PDF Scanner](http://sourceforge.net/projects/naps/), unimaginatively named Snapsy as a cute almost-anagram of NAPS.

I wanted a simple straightforward scan-to-PDF app, and NAPS was very close to what I was after, but it had a few bugs and wasn't quite as streamlined as I thought it could be.

Bug fixes & improvements
------------------------

 * Don't crash on out-of-range brightness & contrast in scanning profile
 * Don't show a null reference error message when cancelling scan or when user cancels out of selecting device via WIA
 * Made scan preview window non-modal
 * Double thumbnail size
 * Add 1:1 zoom button in preview
 * Set PDF author, creator, and title correctly
 * Support for gif and png formats
 * Minor other fixes to profile configuration dialog
 * Show scanned PDF file in system pdf viewer after scanning
 * Default to 90% compression when saving jpegs
 * Add quick scan button which will use last scan profile without prompting
 
Project Status
--------------

I'm no longer working on this project. Suggestions:

* You're better off forking (or checking existing forks) than submitting a pull request to fix a bug; I don't have time to test and review fixes. I'm happy to link to your fork from here if you like.
* You may also like to investigate [NAPS2](http://sourceforge.net/projects/naps2/), which seems to be the original author's 2nd attempt.
 
(I ended up abandoning this project because I changed my document-management workflow to scan and save images with nicely sortable file names for multi-page documents such as `xyz - page 1`, `xyz - page 2` instead.)
