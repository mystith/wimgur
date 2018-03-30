# wimgur
wimgur, a wallhaven / imgur album downloader

# syntax
```
wallhaven [parameters]
parameters qualify as following: website,search
there can be multiple parameters
website = either wallhaven or imgur, capitalization doesn't matter (can also be merge, will elaborate later)
search = any valid search on wallhaven, or any valid album on imgur (or folder name to merge all directory folders into)
if you use something like "merge,folder" as a parameter, it'll merge all directories in the current directory into one specified folder.
```
# dependencies
dependencies are included in the release exe, but heres a list:
```
htmlagilitypack
imgur.net
```
