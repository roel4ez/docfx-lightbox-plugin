using System;
using System.Collections.Immutable;
using System.Composition;
using System.IO;
using System.Linq;
using System.Reflection;
using HtmlAgilityPack;
using Microsoft.DocAsCode.Plugins;

namespace DocFx.Plugin.ImageProcessor
{
    [Export(nameof(ImagePostProcessor), typeof(IPostProcessor))]
    public class ImagePostProcessor: IPostProcessor
    {
        public ImmutableDictionary<string, object> PrepareMetadata(ImmutableDictionary<string, object> metadata)
        {
            return metadata;
        }

        public Manifest Process(Manifest manifest, string outputFolder)
        {
            Console.WriteLine("[ImagePostProcessor] Start processing files (Conceptual only)");
            foreach (var file in manifest.Files.Where(f => f.DocumentType == "Conceptual"))
            {
                foreach (var outputFile in file.OutputFiles)
                {
                    AddLightBoxToImage(Path.Combine(outputFolder,outputFile.Value.RelativePath));
                }
            }

            return manifest;
        }

        /// <summary>
        /// Parse the output file with HtmlAgilityPack, and check if there are `img` nodes.
        /// <para>For each image node, add the required attributes, and create an additional div at the end.</para>
        /// </summary>
        /// <param name="path"></param>
        internal void AddLightBoxToImage(string path)
        {
            Console.WriteLine($"Processing {path}");
            var htmldoc = new HtmlDocument();
            htmldoc.Load(path);
            var i = 0;
            var bodyNode = htmldoc.DocumentNode.SelectSingleNode("//body");
            var firstScriptNode = htmldoc.DocumentNode.SelectNodes("//script").First();
            var imgNodes = htmldoc.DocumentNode.SelectNodes("//img[not(@id=logo)]");//TODO: fix xpath

            var foundImg = false;
            foreach (var imgNode in imgNodes.Where(img => img.Id != "logo"))
            {
                var id = $"img{i++}";
                UpdateImgNode(htmldoc, imgNode, id);

                var node = CreateModalTextNode(id,imgNode.GetAttributeValue("alt",""), imgNode.GetAttributeValue("src",""));
                bodyNode.InsertBefore(node, firstScriptNode);
                foundImg = true;
            }
            if (foundImg) AddRequiredStyle(htmldoc);
            htmldoc.Save(path);
            Console.WriteLine($"Saved {path}");
        }

        /// <summary>
        /// Add the required style to the html page, which makes sure the modal is the same size as the image
        /// </summary>
        /// <param name="htmlDoc"></param>
        private void AddRequiredStyle(HtmlDocument htmlDoc)
        {
            var txt = GetModalTextFromResources("style");
            var styleNode = HtmlNode.CreateNode(txt);

            htmlDoc.DocumentNode.SelectSingleNode("//head").AppendChild(styleNode);

        }

        /// <summary>
        /// Add the needed tags to the `img` node.
        /// </summary>
        /// <param name="htmldoc">the root html document</param>
        /// <param name="imgNode">the img node being processed</param>
        /// <param name="id">the id of the image</param>
        private static void UpdateImgNode(HtmlDocument htmldoc, HtmlNode imgNode, string id)
        {
            var toggle = htmldoc.CreateAttribute("data-toggle", "modal");
            imgNode.Attributes.Add(toggle);
            var target = htmldoc.CreateAttribute("data-target", $"#{id}");
            imgNode.Attributes.Add(target);
            var zoom = htmldoc.CreateAttribute("style", "cursor: zoom-in; cursor: -webkit-zoom-in; cursor: -moz-zoom-in");
            imgNode.Attributes.Add(zoom);

            //use following tags for bootstrap modal
            //data-toggle="modal" data-target="#exampleModal"
        }

        private HtmlNode CreateModalTextNode(string idText, string altText, string src)
        {
            var txt = GetModalTextFromResources("modal");
            txt = txt.Replace("{id}", idText);
            txt = txt.Replace("{title}", altText);
            txt = txt.Replace("{source}", src);

            return HtmlNode.CreateNode(txt);
        }

        /// <summary>
        /// Load the html text which represents the modal dialog
        /// </summary>
        /// <returns></returns>
        private string GetModalTextFromResources(string type)
        {
            var assembly = Assembly.GetExecutingAssembly();
            var resourceName = $"DocFx.Plugin.ImageProcessor.{type}.txt";

            using (var stream = assembly.GetManifestResourceStream(resourceName))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }
    }
}
