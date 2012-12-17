# TFS Fast Export

tfs-fast-export is a one-time tool to export source control history from TFS to Git (git-tfs, git-tf).  There are several other git/tfs integration tools, but many of them only use one tfs branch, and do not include branching information or allow migration of multiple branches.  tfs-fast-export instead renders the fast-import format (http://www.kernel.org/pub/software/scm/git/docs/git-fast-import.html) for use by git fast-import.  This allows git to include information from multiple TFS branches as separate git branches.

# Usage

Update Program.cs to use the appropriate URL and branching information.  Run from the command line and pipe the data into git fast-import in a fresh git workspace.  Expect errors the first time you run this.  This tool has not been fully debugged or cleaned up; it worked for us to migrate once from TFS to Git. Your Mileage May Vary.

# License
Copyright (c) 2012, Stuart Turner; tfs-fast-export is licensed under the Apache 2.0 License

