
# Search-Engine-Indexer

this is the logic for indexing each document we have their urls from crawler part by:
1- fetching html using the url
2- parsing the html to get p tags
3- adding all text to a single string
4- preprocess it (tokenize - stem - remove stope words etc..)

i just put the logic without the database code or the windows forms GUI however i will type the strucutre of it 
it's a one table with 3 columns containing each word in each document and positions of that word in that document
word-doc_id-positions
