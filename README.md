# Kopis Photos
A demo app for the Cloud Technology Group's meeting on IaC and DevOps with ARM templates. This repository demonstrates using ARM templates to deploy a full-stack web application that allows a user to upload photos, analyze them for adult content, create thumbnails, index the metadata, and search the metadata from the web.

Since this is intended to be an educational project, there are branches for each step:
1. Website - This is just a simple ASP.NET Core website with an upload page linked to a storage account, along with an ARM template to deploy all resources needed.
2. Functions - This adds a Functions project that can be automatically deployed along with the Website. There is only one function that will be triggered when a file is uploaded.
3. Durable/Cognitive - This adds Cognitive Services analysis to the Functions project to filter for adult content, create thumbnails, and gather metadata about the images.
4. Search - This adds indexing into Azure Search of the image metadata and a simple display of the uploaded images on the homepage.
