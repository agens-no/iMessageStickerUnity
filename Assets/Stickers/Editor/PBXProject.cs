using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System;
using UnityEditor.iOS.Xcode.PBX;
using UnityEngine;

namespace UnityEditor.iOS.Xcode.Stickers
{
    using PBXBuildFileSection           = KnownSectionBase<PBXBuildFileData>;
    using PBXFileReferenceSection       = KnownSectionBase<PBXFileReferenceData>;
    using PBXGroupSection               = KnownSectionBase<PBXGroupData>;
    using PBXContainerItemProxySection  = KnownSectionBase<PBXContainerItemProxyData>;
    using PBXReferenceProxySection      = KnownSectionBase<PBXReferenceProxyData>;
    using PBXSourcesBuildPhaseSection   = KnownSectionBase<PBXSourcesBuildPhaseData>;
    using PBXFrameworksBuildPhaseSection= KnownSectionBase<PBXFrameworksBuildPhaseData>;
    using PBXResourcesBuildPhaseSection = KnownSectionBase<PBXResourcesBuildPhaseData>;
    using PBXCopyFilesBuildPhaseSection = KnownSectionBase<PBXCopyFilesBuildPhaseData>;
    using PBXShellScriptBuildPhaseSection = KnownSectionBase<PBXShellScriptBuildPhaseData>;
    using PBXVariantGroupSection        = KnownSectionBase<PBXVariantGroupData>;
    using PBXNativeTargetSection        = KnownSectionBase<PBXNativeTargetData>;
    using PBXTargetDependencySection    = KnownSectionBase<PBXTargetDependencyData>;
    using XCBuildConfigurationSection   = KnownSectionBase<XCBuildConfigurationData>;
    using XCConfigurationListSection    = KnownSectionBase<XCConfigurationListData>;
    using UnknownSection                = KnownSectionBase<PBXObjectData>;

    /*// Determines the tree the given path is relative to
    public enum PBXSourceTree
    {
        Absolute,   // The path is absolute
        Source,     // The path is relative to the source folder
        Group,      // The path is relative to the folder it's in. This enum is used only internally,
        // do not use it as function parameter
        Build,      // The path is relative to the build products folder
        Developer,  // The path is relative to the developer folder
        Sdk         // The path is relative to the sdk folder
    };*/

    public class PBXProject
    {
        PBXProjectData m_Data = new PBXProjectData();

        // convenience accessors for public members of data. This is temporary; will be fixed by an interface change
        // of PBXProjectData
        PBXContainerItemProxySection containerItems
        {
            get { return m_Data.containerItems; }
        }

        PBXReferenceProxySection references
        {
            get { return m_Data.references; }
        }

        PBXSourcesBuildPhaseSection sources
        {
            get { return m_Data.sources; }
        }

        PBXFrameworksBuildPhaseSection frameworks
        {
            get { return m_Data.frameworks; }
        }

        PBXResourcesBuildPhaseSection resources
        {
            get { return m_Data.resources; }
        }

        PBXCopyFilesBuildPhaseSection copyFiles
        {
            get { return m_Data.copyFiles; }
        }

        PBXShellScriptBuildPhaseSection shellScripts
        {
            get { return m_Data.shellScripts; }
        }

        PBXNativeTargetSection nativeTargets
        {
            get { return m_Data.nativeTargets; }
        }

        PBXTargetDependencySection targetDependencies
        {
            get { return m_Data.targetDependencies; }
        }

        PBXVariantGroupSection variantGroups
        {
            get { return m_Data.variantGroups; }
        }

        XCBuildConfigurationSection buildConfigs
        {
            get { return m_Data.buildConfigs; }
        }

        XCConfigurationListSection configs
        {
            get { return m_Data.configs; }
        }

        PBXProjectSection projectSection
        {
            get { return m_Data.project; }
        }

        PBXBuildFileData BuildFilesGet(string guid)
        {
            return m_Data.BuildFilesGet(guid);
        }

        void BuildFilesAdd(string targetGuid, PBXBuildFileData buildFile)
        {
            m_Data.BuildFilesAdd(targetGuid, buildFile);
        }

        void BuildFilesRemove(string targetGuid, string fileGuid)
        {
            m_Data.BuildFilesRemove(targetGuid, fileGuid);
        }

        PBXBuildFileData BuildFilesGetForSourceFile(string targetGuid, string fileGuid)
        {
            return m_Data.BuildFilesGetForSourceFile(targetGuid, fileGuid);
        }

        IEnumerable<PBXBuildFileData> BuildFilesGetAll()
        {
            return m_Data.BuildFilesGetAll();
        }

        void FileRefsAdd(string realPath, string projectPath, PBXGroupData parent, PBXFileReferenceData fileRef)
        {
            m_Data.FileRefsAdd(realPath, projectPath, parent, fileRef);
        }

        PBXFileReferenceData FileRefsGet(string guid)
        {
            return m_Data.FileRefsGet(guid);
        }

        PBXFileReferenceData FileRefsGetByRealPath(string path, PBXSourceTree sourceTree)
        {
            return m_Data.FileRefsGetByRealPath(path, sourceTree);
        }

        PBXFileReferenceData FileRefsGetByProjectPath(string path)
        {
            return m_Data.FileRefsGetByProjectPath(path);
        }

        void FileRefsRemove(string guid)
        {
            m_Data.FileRefsRemove(guid);
        }

        PBXGroupData GroupsGet(string guid)
        {
            return m_Data.GroupsGet(guid);
        }

        PBXGroupData GroupsGetByChild(string childGuid)
        {
            return m_Data.GroupsGetByChild(childGuid);
        }

        PBXGroupData GroupsGetMainGroup()
        {
            return m_Data.GroupsGetMainGroup();
        }

        PBXGroupData GroupsGetByProjectPath(string sourceGroup)
        {
            return m_Data.GroupsGetByProjectPath(sourceGroup);
        }

        void GroupsAdd(string projectPath, PBXGroupData parent, PBXGroupData gr)
        {
            m_Data.GroupsAdd(projectPath, parent, gr);
        }

        void GroupsAddDuplicate(PBXGroupData gr)
        {
            m_Data.GroupsAddDuplicate(gr);
        }

        void GroupsRemove(string guid)
        {
            m_Data.GroupsRemove(guid);
        }

        FileGUIDListBase BuildSectionAny(PBXNativeTargetData target, string path, bool isFolderRef)
        {
            return m_Data.BuildSectionAny(target, path, isFolderRef);
        }


        public static string GetPBXProjectPath(string buildPath)
        {
            return PBX.Utils.CombinePaths(buildPath, "Unity-iPhone.xcodeproj/project.pbxproj");
        }

        public static string GetUnityTargetName()
        {
            return "Unity-iPhone";
        }

        public static string GetUnityTestTargetName()
        {
            return "Unity-iPhone Tests";
        }

        internal string ProjectGuid()
        {
            return projectSection.project.guid;
        }

        /// Returns a guid identifying native target with name @a name
        public string TargetGuidByName(string name)
        {
            foreach (var entry in nativeTargets.GetEntries())
                if (entry.Value.name == name)
                    return entry.Key;
            return null;
        }

        /// Returns the name of the native target identified with guid @a guid
        public string TargetNameByGuid(string guid)
        {
            var target = nativeTargets[guid];
            if (target == null)
                return null;
            else
                return target.name;
        }

        public static bool IsKnownExtension(string ext)
        {
            return FileTypeUtils.IsKnownExtension(ext);
        }

        public static bool IsBuildable(string ext)
        {
            return FileTypeUtils.IsBuildableFile(ext);
        }

        // The same file can be referred to by more than one project path.
        private string AddFileImpl(string path, string projectPath, PBXSourceTree tree, bool isFolderReference)
        {
            path = PBX.Utils.FixSlashesInPath(path);
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);

            if (!isFolderReference && Path.GetExtension(path) != Path.GetExtension(projectPath))
                throw new Exception("Project and real path extensions do not match");

            string guid = FindFileGuidByProjectPath(projectPath);
            if (guid == null)
                guid = FindFileGuidByRealPath(path);
            if (guid == null)
            {
                PBXFileReferenceData fileRef;
                if (isFolderReference)
                    fileRef = PBXFileReferenceData.CreateFromFolderReference(path, PBX.Utils.GetFilenameFromPath(projectPath), tree);
                else
                    fileRef = PBXFileReferenceData.CreateFromFile(path, PBX.Utils.GetFilenameFromPath(projectPath), tree);
                PBXGroupData parent = CreateSourceGroup(PBX.Utils.GetDirectoryFromPath(projectPath));
                parent.children.AddGUID(fileRef.guid);
                FileRefsAdd(path, projectPath, parent, fileRef);
                guid = fileRef.guid;
            }
            return guid;
        }

        // The extension of the files identified by path and projectPath must be the same.
        public string AddFile(string path, string projectPath)
        {
            return AddFileImpl(path, projectPath, PBXSourceTree.Source, false);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string AddFile(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return AddFileImpl(path, projectPath, sourceTree, false);
        }

        public string AddFolderReference(string path, string projectPath)
        {
            return AddFileImpl(path, projectPath, PBXSourceTree.Source, true);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string AddFolderReference(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return AddFileImpl(path, projectPath, sourceTree, true);
        }

        private void AddBuildFileImpl(string targetGuid, string fileGuid, bool weak, string compileFlags)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            PBXFileReferenceData fileRef = FileRefsGet(fileGuid);

            string ext = Path.GetExtension(fileRef.path);

            if (FileTypeUtils.IsBuildable(ext, fileRef.isFolderReference) &&
                BuildFilesGetForSourceFile(targetGuid, fileGuid) == null)
            {
                PBXBuildFileData buildFile = PBXBuildFileData.CreateFromFile(fileGuid, weak, compileFlags);
                BuildFilesAdd(targetGuid, buildFile);
                BuildSectionAny(target, ext, fileRef.isFolderReference).files.AddGUID(buildFile.guid);
            }
        }

        public void AddFileToBuild(string targetGuid, string fileGuid)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, null);
        }

        public void AddFileToBuildWithFlags(string targetGuid, string fileGuid, string compileFlags)
        {
            AddBuildFileImpl(targetGuid, fileGuid, false, compileFlags);
        }

        // returns null on error
        // FIXME: at the moment returns all flags as the first element of the array
        public List<string> GetCompileFlagsForFile(string targetGuid, string fileGuid)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return null;
            if (buildFile.compileFlags == null)
                return new List<string>();
            return new List<string> {buildFile.compileFlags};
        }

        public void SetCompileFlagsForFile(string targetGuid, string fileGuid, List<string> compileFlags)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            if (compileFlags == null)
                buildFile.compileFlags = null;
            else
                buildFile.compileFlags = string.Join(" ", compileFlags.ToArray());
        }

        public void AddAssetTagForFile(string targetGuid, string fileGuid, string tag)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            if (!buildFile.assetTags.Contains(tag))
                buildFile.assetTags.Add(tag);
            if (!projectSection.project.knownAssetTags.Contains(tag))
                projectSection.project.knownAssetTags.Add(tag);
        }

        public void RemoveAssetTagForFile(string targetGuid, string fileGuid, string tag)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            buildFile.assetTags.Remove(tag);
            // remove from known tags if this was the last one
            foreach (var buildFile2 in BuildFilesGetAll())
            {
                if (buildFile2.assetTags.Contains(tag))
                    return;
            }
            projectSection.project.knownAssetTags.Remove(tag);
        }

        public void AddAssetTagToDefaultInstall(string targetGuid, string tag)
        {
            if (!projectSection.project.knownAssetTags.Contains(tag))
                return;
            AddBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", tag);
        }

        public void RemoveAssetTagFromDefaultInstall(string targetGuid, string tag)
        {
            UpdateBuildProperty(targetGuid, "ON_DEMAND_RESOURCES_INITIAL_INSTALL_TAGS", null, new string[] {tag});
        }

        public void RemoveAssetTag(string tag)
        {
            foreach (var buildFile in BuildFilesGetAll())
                buildFile.assetTags.Remove(tag);
            foreach (var targetGuid in nativeTargets.GetGuids())
                RemoveAssetTagFromDefaultInstall(targetGuid, tag);
            projectSection.project.knownAssetTags.Remove(tag);
        }

        public bool ContainsFileByRealPath(string path)
        {
            return FindFileGuidByRealPath(path) != null;
        }

        // sourceTree must not be PBXSourceTree.Group
        public bool ContainsFileByRealPath(string path, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            return FindFileGuidByRealPath(path, sourceTree) != null;
        }

        public bool ContainsFileByProjectPath(string path)
        {
            return FindFileGuidByProjectPath(path) != null;
        }

        public bool HasFramework(string framework)
        {
            return ContainsFileByRealPath("System/Library/Frameworks/" + framework);
        }

        /// The framework must be specified with the '.framework' extension
        public void AddFrameworkToProject(string targetGuid, string framework, bool weak)
        {
            string fileGuid = AddFile("System/Library/Frameworks/" + framework, "Frameworks/" + framework, PBXSourceTree.Sdk);
            AddBuildFileImpl(targetGuid, fileGuid, weak, null);
        }

        /// The framework must be specified with the '.framework' extension
        // FIXME: targetGuid is ignored at the moment
        public void RemoveFrameworkFromProject(string targetGuid, string framework)
        {
            string fileGuid = FindFileGuidByRealPath("System/Library/Frameworks/" + framework);
            if (fileGuid != null)
                RemoveFile(fileGuid);
        }

        // sourceTree must not be PBXSourceTree.Group
        public string FindFileGuidByRealPath(string path, PBXSourceTree sourceTree)
        {
            //if (sourceTree == PBXSourceTree.Group)
            //    throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = PBX.Utils.FixSlashesInPath(path);
            var fileRef = FileRefsGetByRealPath(path, sourceTree);
            if (fileRef != null)
                return fileRef.guid;
            return null;
        }

        public string FindFileGuidByRealPath(string path)
        {
            path = PBX.Utils.FixSlashesInPath(path);

            foreach (var tree in FileTypeUtils.AllAbsoluteSourceTrees())
            {
                string res = FindFileGuidByRealPath(path, tree);
                if (res != null)
                    return res;
            }
            return null;
        }

        public string FindFileGuidByProjectPath(string path)
        {
            path = PBX.Utils.FixSlashesInPath(path);
            var fileRef = FileRefsGetByProjectPath(path);
            if (fileRef != null)
                return fileRef.guid;
            return null;
        }

        public void RemoveFileFromBuild(string targetGuid, string fileGuid)
        {
            var buildFile = BuildFilesGetForSourceFile(targetGuid, fileGuid);
            if (buildFile == null)
                return;
            BuildFilesRemove(targetGuid, fileGuid);

            string buildGuid = buildFile.guid;
            if (buildGuid != null)
            {
                foreach (var section in sources.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in resources.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in copyFiles.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
                foreach (var section in frameworks.GetEntries())
                    section.Value.files.RemoveGUID(buildGuid);
            }
        }

        public void RemoveFile(string fileGuid)
        {
            if (fileGuid == null)
                return;

            // remove from parent
            PBXGroupData parent = GroupsGetByChild(fileGuid);
            if (parent != null)
                parent.children.RemoveGUID(fileGuid);
            RemoveGroupIfEmpty(parent);

            // remove actual file
            foreach (var target in nativeTargets.GetEntries())
                RemoveFileFromBuild(target.Value.guid, fileGuid);
            FileRefsRemove(fileGuid);
        }

        void RemoveGroupIfEmpty(PBXGroupData gr)
        {
            if (gr.children.Count == 0 && gr != GroupsGetMainGroup())
            {
                // remove from parent
                PBXGroupData parent = GroupsGetByChild(gr.guid);
                parent.children.RemoveGUID(gr.guid);
                RemoveGroupIfEmpty(parent);

                // remove actual group
                GroupsRemove(gr.guid);
            }
        }

        private void RemoveGroupChildrenRecursive(PBXGroupData parent)
        {
            List<string> children = new List<string>(parent.children);
            parent.children.Clear();
            foreach (string guid in children)
            {
                PBXFileReferenceData file = FileRefsGet(guid);
                if (file != null)
                {
                    foreach (var target in nativeTargets.GetEntries())
                        RemoveFileFromBuild(target.Value.guid, guid);
                    FileRefsRemove(guid);
                    continue;
                }

                PBXGroupData gr = GroupsGet(guid);
                if (gr != null)
                {
                    RemoveGroupChildrenRecursive(gr);
                    GroupsRemove(gr.guid);
                    continue;
                }
            }
        }

        internal void RemoveFilesByProjectPathRecursive(string projectPath)
        {
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);
            PBXGroupData gr = GroupsGetByProjectPath(projectPath);
            if (gr == null)
                return;
            RemoveGroupChildrenRecursive(gr);
            RemoveGroupIfEmpty(gr);
        }

        // Returns null on error
        internal List<string> GetGroupChildrenFiles(string projectPath)
        {
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);
            PBXGroupData gr = GroupsGetByProjectPath(projectPath);
            if (gr == null)
                return null;
            var res = new List<string>();
            foreach (var guid in gr.children)
            {
                var fileRef = FileRefsGet(guid);
                if (fileRef != null)
                    res.Add(fileRef.name);
            }
            return res;
        }

        private PBXGroupData GetPBXGroupChildByName(PBXGroupData group, string name)
        {
            foreach (string guid in group.children)
            {
                var gr = GroupsGet(guid);
                if (gr != null && gr.name == name)
                    return gr;
            }
            return null;
        }

        /// Creates source group identified by sourceGroup, if needed, and returns it.
        /// If sourceGroup is empty or null, root group is returned
        private PBXGroupData CreateSourceGroup(string sourceGroup)
        {
            sourceGroup = PBX.Utils.FixSlashesInPath(sourceGroup);

            if (sourceGroup == null || sourceGroup == "")
                return GroupsGetMainGroup();

            PBXGroupData gr = GroupsGetByProjectPath(sourceGroup);
            if (gr != null)
                return gr;

            // the group does not exist -- create new
            gr = GroupsGetMainGroup();

            var elements = PBX.Utils.SplitPath(sourceGroup);
            string projectPath = null;
            foreach (string pathEl in elements)
            {
                if (projectPath == null)
                    projectPath = pathEl;
                else
                    projectPath += "/" + pathEl;

                PBXGroupData child = GetPBXGroupChildByName(gr, pathEl);
                if (child != null)
                    gr = child;
                else
                {
                    PBXGroupData newGroup = PBXGroupData.Create(pathEl, pathEl, PBXSourceTree.Group);
                    gr.children.AddGUID(newGroup.guid);
                    GroupsAdd(projectPath, gr, newGroup);
                    gr = newGroup;
                }
            }
            return gr;
        }

        // sourceTree must not be PBXSourceTree.Group
        public void AddExternalProjectDependency(string path, string projectPath, PBXSourceTree sourceTree)
        {
            if (sourceTree == PBXSourceTree.Group)
                throw new Exception("sourceTree must not be PBXSourceTree.Group");
            path = PBX.Utils.FixSlashesInPath(path);
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);

            // note: we are duplicating products group for the project reference. Otherwise Xcode crashes.
            PBXGroupData productGroup = PBXGroupData.CreateRelative("Products");
            GroupsAddDuplicate(productGroup); // don't use GroupsAdd here

            PBXFileReferenceData fileRef = PBXFileReferenceData.CreateFromFile(path, Path.GetFileName(projectPath),
                sourceTree);
            FileRefsAdd(path, projectPath, null, fileRef);
            CreateSourceGroup(PBX.Utils.GetDirectoryFromPath(projectPath)).children.AddGUID(fileRef.guid);

            projectSection.project.AddReference(productGroup.guid, fileRef.guid);
        }

        /** This function must be called only after the project the library is in has
            been added as a dependency via AddExternalProjectDependency. projectPath must be
            the same as the 'path' parameter passed to the AddExternalProjectDependency.
            remoteFileGuid must be the guid of the referenced file as specified in
            PBXFileReference section of the external project

            TODO: what. is remoteInfo entry in PBXContainerItemProxy? Is in referenced project name or
            referenced library name without extension?
        */
        public void AddExternalLibraryDependency(string targetGuid, string filename, string remoteFileGuid, string projectPath,
            string remoteInfo)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            filename = PBX.Utils.FixSlashesInPath(filename);
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);

            // find the products group to put the new library in
            string projectGuid = FindFileGuidByRealPath(projectPath);
            if (projectGuid == null)
                throw new Exception("No such project");

            string productsGroupGuid = null;
            foreach (var proj in projectSection.project.projectReferences)
            {
                if (proj.projectRef == projectGuid)
                {
                    productsGroupGuid = proj.group;
                    break;
                }
            }

            if (productsGroupGuid == null)
                throw new Exception("Malformed project: no project in project references");

            PBXGroupData productGroup = GroupsGet(productsGroupGuid);

            // verify file extension
            string ext = Path.GetExtension(filename);
            if (!FileTypeUtils.IsBuildableFile(ext))
                throw new Exception("Wrong file extension");

            // create ContainerItemProxy object
            var container = PBXContainerItemProxyData.Create(projectGuid, "2", remoteFileGuid, remoteInfo);
            containerItems.AddEntry(container);

            // create a reference and build file for the library
            string typeName = FileTypeUtils.GetTypeName(ext);

            var libRef = PBXReferenceProxyData.Create(filename, typeName, container.guid, "BUILT_PRODUCTS_DIR");
            references.AddEntry(libRef);
            PBXBuildFileData libBuildFile = PBXBuildFileData.CreateFromFile(libRef.guid, false, null);
            BuildFilesAdd(targetGuid, libBuildFile);
            BuildSectionAny(target, ext, false).files.AddGUID(libBuildFile.guid);

            // add to products folder
            productGroup.children.AddGUID(libRef.guid);
        }

        private void SetDefaultAppExtensionReleaseBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "YES");
            config.AddProperty("DEVELOPMENT_TEAM", "");
            config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
            config.AddProperty("VALIDATE_PRODUCT", "YES");
        }

        private void SetDefaultAppExtensionDebugBuildFlags(XCBuildConfigurationData config, string infoPlistPath)
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "NO");
            config.AddProperty("DEVELOPMENT_TEAM", "");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
            config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
            config.AddProperty("GCC_SYMBOLS_PRIVATE_EXTERN", "NO");
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            config.AddProperty("INFOPLIST_FILE", infoPlistPath);
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "8.0");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "$(inherited)");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/Frameworks");
            config.AddProperty("LD_RUNPATH_SEARCH_PATHS", "@executable_path/../../Frameworks");
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
            config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
        }

        // Returns the guid of the new target
        public string CreateNewTarget(string name, string ext, string type)
        {
            // create build configurations
            var releaseBuildConfig = XCBuildConfigurationData.Create("Release");
            buildConfigs.AddEntry(releaseBuildConfig);

            var debugBuildConfig = XCBuildConfigurationData.Create("Debug");
            buildConfigs.AddEntry(debugBuildConfig);

            var buildConfigList = XCConfigurationListData.Create();
            configs.AddEntry(buildConfigList);
            buildConfigList.buildConfigs.AddGUID(releaseBuildConfig.guid);
            buildConfigList.buildConfigs.AddGUID(debugBuildConfig.guid);

            // create build file reference
            string fullName = name + ext;
            var productFileRef = AddFile(fullName, "Products/" + fullName, PBXSourceTree.Build);
            var newTarget = PBXNativeTargetData.Create(name, productFileRef, type, buildConfigList.guid);
            nativeTargets.AddEntry(newTarget);
            projectSection.project.targets.Add(newTarget.guid);

            return newTarget.guid;
        }

        // Returns the guid of the new target
        public string AddAppExtension(string mainTarget, string name, string type, string infoPlistPath)
        {
            string ext = ".appex";
            string newTargetGuid = CreateNewTarget(name, ext, type);
            var newTarget = nativeTargets[newTargetGuid];

            SetDefaultAppExtensionReleaseBuildFlags(buildConfigs[BuildConfigByName(newTarget.guid, "Release")], infoPlistPath);
            SetDefaultAppExtensionDebugBuildFlags(buildConfigs[BuildConfigByName(newTarget.guid, "Debug")], infoPlistPath);

            var sourcesBuildPhase = PBXSourcesBuildPhaseData.Create();
            sources.AddEntry(sourcesBuildPhase);
            newTarget.phases.AddGUID(sourcesBuildPhase.guid);

            var resourcesBuildPhase = PBXResourcesBuildPhaseData.Create();
            resources.AddEntry(resourcesBuildPhase);
            newTarget.phases.AddGUID(resourcesBuildPhase.guid);

            var frameworksBuildPhase = PBXFrameworksBuildPhaseData.Create();
            frameworks.AddEntry(frameworksBuildPhase);
            newTarget.phases.AddGUID(frameworksBuildPhase.guid);

            var copyFilesBuildPhase = PBXCopyFilesBuildPhaseData.Create("Embed App Extensions", "13");
            copyFiles.AddEntry(copyFilesBuildPhase);
            nativeTargets[mainTarget].phases.AddGUID(copyFilesBuildPhase.guid);

            var containerProxy = PBXContainerItemProxyData.Create(projectSection.project.guid, "1", newTarget.guid, name);
            containerItems.AddEntry(containerProxy);

            var targetDependency = PBXTargetDependencyData.Create(newTarget.guid, containerProxy.guid);
            targetDependencies.AddEntry(targetDependency);

            nativeTargets[mainTarget].dependencies.AddGUID(targetDependency.guid);

            var buildAppCopy = PBXBuildFileData.CreateFromFile(FindFileGuidByProjectPath("Products/" + name + ext), false, "");
            BuildFilesAdd(mainTarget, buildAppCopy);
            copyFilesBuildPhase.files.AddGUID(buildAppCopy.guid);

            AddFile(infoPlistPath, infoPlistPath, PBXSourceTree.Source);

            return newTarget.guid;
        }

        public string BuildConfigByName(string targetGuid, string name)
        {
            PBXNativeTargetData target = nativeTargets[targetGuid];
            foreach (string guid in configs[target.buildConfigList].buildConfigs)
            {
                var buildConfig = buildConfigs[guid];
                if (buildConfig != null && buildConfig.name == name)
                    return buildConfig.guid;
            }
            return null;
        }

        public void AddFileToResourcesForTarget(string targetGuid, string filePath)
        {
            var target = nativeTargets[targetGuid];

            if (target == null)
                throw new Exception("Invalid target guid");

            Dictionary<string, PBXResourcesBuildPhaseData> resourcesDictionary = (Dictionary<string, PBXResourcesBuildPhaseData>) resources.GetEntries();
            PBXResourcesBuildPhaseData resource = null;
            foreach (var section in target.phases)
            {
                if (resourcesDictionary.ContainsKey(section))
                {
                    resource = resourcesDictionary[section];
                }
            }

            if (resource == null)
                throw new Exception("No PBXResourcesBuildPhaseData for target");

            var buildFileData = PBXBuildFileData.CreateFromFile(FindFileGuidByProjectPath(filePath), false, "");
            BuildFilesAdd(targetGuid, buildFileData);
            resource.files.AddGUID(buildFileData.guid);
        }

        string GetConfigListForTarget(string targetGuid)
        {
            if (targetGuid == projectSection.project.guid)
                return projectSection.project.buildConfigList;
            else
                return nativeTargets[targetGuid].buildConfigList;
        }

        // Adds an item to a build property that contains a value list. Duplicate build properties
        // are ignored. Values for name "LIBRARY_SEARCH_PATHS" are quoted if they contain spaces.
        // targetGuid may refer to PBXProject object
        public void AddBuildProperty(string targetGuid, string name, string value)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                AddBuildPropertyForConfig(guid, name, value);
        }

        public void AddBuildProperty(IEnumerable<string> targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                AddBuildProperty(t, name, value);
        }

        public void AddBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].AddProperty(name, value);
        }

        public void AddBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                AddBuildPropertyForConfig(guid, name, value);
        }

        // targetGuid may refer to PBXProject object
        public void SetBuildProperty(string targetGuid, string name, string value)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                SetBuildPropertyForConfig(guid, name, value);
        }

        public void SetBuildProperty(IEnumerable<string> targetGuids, string name, string value)
        {
            foreach (string t in targetGuids)
                SetBuildProperty(t, name, value);
        }

        public void SetBuildPropertyForConfig(string configGuid, string name, string value)
        {
            buildConfigs[configGuid].SetProperty(name, value);
        }

        public void SetBuildPropertyForConfig(IEnumerable<string> configGuids, string name, string value)
        {
            foreach (string guid in configGuids)
                SetBuildPropertyForConfig(guid, name, value);
        }

        internal void RemoveBuildProperty(string targetGuid, string name)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                RemoveBuildPropertyForConfig(guid, name);
        }

        internal void RemoveBuildProperty(IEnumerable<string> targetGuids, string name)
        {
            foreach (string t in targetGuids)
                RemoveBuildProperty(t, name);
        }

        internal void RemoveBuildPropertyForConfig(string configGuid, string name)
        {
            buildConfigs[configGuid].RemoveProperty(name);
        }

        internal void RemoveBuildPropertyForConfig(IEnumerable<string> configGuids, string name)
        {
            foreach (string guid in configGuids)
                RemoveBuildPropertyForConfig(guid, name);
        }

        /// Interprets the value of the given property as a set of space-delimited strings, then
        /// removes strings equal to items to removeValues and adds strings in addValues.
        public void UpdateBuildProperty(string targetGuid, string name,
            IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string guid in configs[GetConfigListForTarget(targetGuid)].buildConfigs)
                UpdateBuildPropertyForConfig(guid, name, addValues, removeValues);
        }

        public void UpdateBuildProperty(IEnumerable<string> targetGuids, string name,
            IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string t in targetGuids)
                UpdateBuildProperty(t, name, addValues, removeValues);
        }

        public void UpdateBuildPropertyForConfig(string configGuid, string name,
            IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            var config = buildConfigs[configGuid];
            if (config != null)
            {
                if (removeValues != null)
                    foreach (var v in removeValues)
                        config.RemovePropertyValue(name, v);
                if (addValues != null)
                    foreach (var v in addValues)
                        config.AddProperty(name, v);
            }
        }

        public void UpdateBuildPropertyForConfig(IEnumerable<string> configGuids, string name,
            IEnumerable<string> addValues, IEnumerable<string> removeValues)
        {
            foreach (string guid in configGuids)
                UpdateBuildProperty(guid, name, addValues, removeValues);
        }

        public void ReadFromFile(string path)
        {
            ReadFromString(File.ReadAllText(path));
        }

        public void ReadFromString(string src)
        {
            TextReader sr = new StringReader(src);
            ReadFromStream(sr);
        }

        public void ReadFromStream(TextReader sr)
        {
            m_Data.ReadFromStream(sr);
        }

        public void WriteToFile(string path)
        {
            File.WriteAllText(path, WriteToString());
        }

        public void WriteToStream(TextWriter sw)
        {
            sw.Write(WriteToString());
        }

        public string WriteToString()
        {
            return m_Data.WriteToString();
        }

        public static void AddStickerExtensionToXcodeProject(string extensionSourcePath, string pathToBuiltProject, string extensionGroupName, string extensionBundleId, string teamId, string bundleVersion, string buildNumber, string targetDevice, bool auomaticSigning, string provisioningProfile = null, string provisioningProfileSpecifier = null)
        {

            string projPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);
            PBXProject proj = new PBXProject();
            proj.ReadFromString(File.ReadAllText(projPath));
            var unityIphone = proj.TargetGuidByName(PBXProject.GetUnityTargetName());

            CopyDirectory(extensionSourcePath, pathToBuiltProject + extensionGroupName + "/", false);
            if (proj.HasFramework("Messages.framework"))
            {
                Debug.LogWarning("Xcode already contains a messages framework.");
            }
            proj.AddFrameworkToProject(unityIphone, "Messages.framework", true);

            var stickerExtensionName = "Unity-iPhone-Stickers";
            string ext = ".appex";
            string infoPlistPath = extensionGroupName + "/Info.plist";
            if (proj.ContainsExtensionTarget(stickerExtensionName))
            {
                Debug.LogWarning("Sticker extension already added");
            }
            else
            {
                var unityiPhoneStickers = proj.CreateExtensionTarget(stickerExtensionName, ext, "com.apple.product-type.app-extension.messages-sticker-pack");
                proj.SetStickerExtensionReleaseBuildFlags(
                    proj.buildConfigs[proj.BuildConfigByName(unityiPhoneStickers.guid, "Release")],
                    infoPlistPath,
                    extensionBundleId,
                    teamId,
                    targetDevice,
                    auomaticSigning,
                    provisioningProfile,
                    provisioningProfileSpecifier
                );
                proj.SetStickerExtensionDebugBuildFlags(
                    proj.buildConfigs[proj.BuildConfigByName(unityiPhoneStickers.guid, "Debug")],
                    infoPlistPath,
                    extensionBundleId,
                    teamId,
                    targetDevice,
                    auomaticSigning,
                    provisioningProfile,
                    provisioningProfileSpecifier
                );
#if UNITY_5_5_OR_NEWER
                proj.SetStickerExtensionReleaseBuildFlags(
                    proj.buildConfigs[proj.BuildConfigByName(unityiPhoneStickers.guid, "ReleaseForProfiling")],
                    infoPlistPath,
                    extensionBundleId,
                    teamId,
                    targetDevice,
                    auomaticSigning,
                    provisioningProfile,
                    provisioningProfileSpecifier
                );
                proj.SetStickerExtensionReleaseBuildFlags(
                    proj.buildConfigs[proj.BuildConfigByName(unityiPhoneStickers.guid, "ReleaseForRunning")],
                    infoPlistPath,
                    extensionBundleId,
                    teamId,
                    targetDevice,
                    auomaticSigning,
                    provisioningProfile,
                    provisioningProfileSpecifier
                );
#endif
                
                proj.projectSection.project.teamIDs.Add(unityiPhoneStickers.guid, teamId);

                var resourcesBuildPhase = PBXResourcesBuildPhaseData.Create();
                proj.resources.AddEntry(resourcesBuildPhase);
                unityiPhoneStickers.phases.AddGUID(resourcesBuildPhase.guid);

                proj.AddFileToBuild(unityiPhoneStickers.guid, proj.AddFileCustom(extensionSourcePath + "Stickers.xcassets", extensionGroupName + "/Stickers.xcassets", PBXSourceTree.Group, false));
                proj.AddFileCustom(extensionSourcePath + "Info.plist", extensionGroupName + "/Info.plist", PBXSourceTree.Group, false);

                var copyFilesBuildPhase = PBXCopyFilesBuildPhaseData.Create("Embed App Extensions", "13");
                proj.copyFiles.AddEntry(copyFilesBuildPhase);
                proj.nativeTargets[unityIphone].phases.AddGUID(copyFilesBuildPhase.guid);

                var containerProxy = PBXContainerItemProxyData.Create(proj.projectSection.project.guid, "1", unityiPhoneStickers.guid, stickerExtensionName);
                proj.containerItems.AddEntry(containerProxy);

                var targetDependency = PBXTargetDependencyData.Create(unityiPhoneStickers.guid, containerProxy.guid);
                proj.targetDependencies.AddEntry(targetDependency);
                proj.nativeTargets[unityIphone].dependencies.AddGUID(targetDependency.guid);

                var buildAppCopy = PBXBuildFileData.CreateFromFile(proj.FindFileGuidByProjectPath("Products/" + stickerExtensionName + ext), false, "");
                proj.BuildFilesAdd(unityIphone, buildAppCopy);
                copyFilesBuildPhase.files.AddGUID(buildAppCopy.guid);


                File.WriteAllText(projPath, proj.WriteToString());
            }

            // need to set the version numbers to match the main app
            PlistDocument plist = new PlistDocument();
            string stickerPlistPath = projPath.Substring(0, projPath.LastIndexOf("/")) + "/../" + extensionGroupName + "/Info.plist";
            plist.ReadFromString(File.ReadAllText(stickerPlistPath));
            PlistElementDict rootDict = plist.root;
            rootDict.SetString("CFBundleShortVersionString", bundleVersion);
            rootDict.SetString("CFBundleVersion", buildNumber);
            File.WriteAllText(stickerPlistPath, plist.WriteToString());

        }

        private PBXNativeTargetData CreateExtensionTarget(string name, string ext, string type)
        {
            // create build configurations
            var releaseBuildConfig = XCBuildConfigurationData.Create("Release");
            buildConfigs.AddEntry(releaseBuildConfig);

            var debugBuildConfig = XCBuildConfigurationData.Create("Debug");
            buildConfigs.AddEntry(debugBuildConfig);
            
#if UNITY_5_5_OR_NEWER
            var releaseForRunning = XCBuildConfigurationData.Create("ReleaseForRunning");
            buildConfigs.AddEntry(releaseForRunning);
            
            var releaseForProfiling = XCBuildConfigurationData.Create("ReleaseForProfiling");
            buildConfigs.AddEntry(releaseForProfiling);
#endif

            var buildConfigList = XCConfigurationListData.Create();
            configs.AddEntry(buildConfigList);
            buildConfigList.buildConfigs.AddGUID(releaseBuildConfig.guid);
            buildConfigList.buildConfigs.AddGUID(debugBuildConfig.guid);
            
#if UNITY_5_5_OR_NEWER
            buildConfigList.buildConfigs.AddGUID(releaseForRunning.guid);
            buildConfigList.buildConfigs.AddGUID(releaseForProfiling.guid);
#endif

            // create build file reference
            string fullName = name + ext;
            var productFileRef = AddFileCustom(fullName, "Products/" + fullName, PBXSourceTree.Build, false, false);
            var newTarget = PBXNativeTargetData.Create(name, productFileRef, type, buildConfigList.guid);
            nativeTargets.AddEntry(newTarget);
            projectSection.project.targets.Add(newTarget.guid);

            return newTarget;
        }

        private bool ContainsExtensionTarget(string name)
        {
            foreach (var entry in nativeTargets.GetEntries())
            {
                if (entry.Value.name == name)
                {
                    return true;
                }
            }
            return false;
        }

        private void SetStickerExtensionBuildFlags(XCBuildConfigurationData config, string infoPlistPath, string bundleId, string teamId, string targetDevice, bool automaticSigning, string provisioningProfile = "", string provisioningProfileSpecifier = "")
        {
            config.AddProperty("ALWAYS_SEARCH_USER_PATHS", "NO");
            config.AddProperty("ASSETCATALOG_COMPILER_APPICON_NAME", "iMessage App Icon");
            config.AddProperty("CLANG_ANALYZER_NONNULL", "YES");
            config.AddProperty("CLANG_CXX_LANGUAGE_STANDARD", "gnu++0x");
            config.AddProperty("CLANG_CXX_LIBRARY", "libc++");
            config.AddProperty("CLANG_ENABLE_MODULES", "YES");
            config.AddProperty("CLANG_ENABLE_OBJC_ARC", "YES");
            config.AddProperty("CLANG_WARN_BOOL_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_CONSTANT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_DIRECT_OBJC_ISA_USAGE", "YES_ERROR");
            config.AddProperty("CLANG_WARN_DOCUMENTATION_COMMENTS", "YES");
            config.AddProperty("CLANG_WARN_EMPTY_BODY", "YES");
            config.AddProperty("CLANG_WARN_ENUM_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_INFINITE_RECURSION", "YES");
            config.AddProperty("CLANG_WARN_INT_CONVERSION", "YES");
            config.AddProperty("CLANG_WARN_OBJC_ROOT_CLASS", "YES_ERROR");
            config.AddProperty("CLANG_WARN_SUSPICIOUS_MOVES", "YES");
            config.AddProperty("CLANG_WARN_UNREACHABLE_CODE", "YES");
            config.AddProperty("CLANG_WARN__DUPLICATE_METHOD_MATCH", "YES");
            config.AddProperty("COPY_PHASE_STRIP", "NO");
            
            config.AddProperty("GCC_WARN_64_TO_32_BIT_CONVERSION", "YES");
            config.AddProperty("GCC_WARN_ABOUT_RETURN_TYPE", "YES_ERROR");
            config.AddProperty("GCC_WARN_UNDECLARED_SELECTOR", "YES");
            config.AddProperty("GCC_WARN_UNINITIALIZED_AUTOS", "YES_AGGRESSIVE");
            config.AddProperty("GCC_WARN_UNUSED_FUNCTION", "YES");
            
            config.AddProperty("IPHONEOS_DEPLOYMENT_TARGET", "10.0"); // stickers introduced in 10.0
            config.AddProperty("PRODUCT_NAME", "$(TARGET_NAME)");
            config.AddProperty("SKIP_INSTALL", "YES");
            if (automaticSigning)
            {
                config.AddProperty("CODE_SIGN_IDENTITY[sdk=iphoneos*]", Debug.isDebugBuild ? "iPhone Developer" : "iPhone Distribution");
            }
            config.AddProperty("INFOPLIST_FILE", infoPlistPath); // e.g. relative to source root "Stickers/Info.plist"
            config.AddProperty("PRODUCT_BUNDLE_IDENTIFIER", bundleId);
            config.AddProperty("TARGETED_DEVICE_FAMILY", targetDevice); // e.g. 1,2 universal
            config.AddProperty("DEVELOPMENT_TEAM", teamId);
            config.AddProperty("CODE_SIGN_STYLE", automaticSigning ? "Automatic" : "Manual");
            if (!automaticSigning && !string.IsNullOrEmpty(provisioningProfile))
            {
                config.AddProperty("PROVISIONING_PROFILE", provisioningProfile);
            }
            if (!automaticSigning && !string.IsNullOrEmpty(provisioningProfileSpecifier))
            {
                config.AddProperty("PROVISIONING_PROFILE_SPECIFIER", provisioningProfileSpecifier);
            }
        }

        private void SetStickerExtensionReleaseBuildFlags(XCBuildConfigurationData config, string infoPlistPath, string bundleId, string teamId, string targetDevice, bool automaticSigning, string provisioningProfile = "", string provisioningProfileSpecifier = "")
        {
            SetStickerExtensionBuildFlags(config, infoPlistPath, bundleId, teamId, targetDevice, automaticSigning, provisioningProfile, provisioningProfileSpecifier);
            config.AddProperty("DEBUG_INFORMATION_FORMAT", "dwarf-with-dsym");
            
            config.AddProperty("ENABLE_NS_ASSERTIONS", "NO");
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_NO_COMMON_BLOCKS", "YES");
            
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "NO");
            
            config.AddProperty("VALIDATE_PRODUCT", "YES");
        }

        private void SetStickerExtensionDebugBuildFlags(XCBuildConfigurationData config, string infoPlistPath, string bundleId, string teamId, string targetDevice, bool automaticSigning, string provisioningProfile = "", string provisioningProfileSpecifier = "")
        {
            SetStickerExtensionBuildFlags(config, infoPlistPath, bundleId, teamId, targetDevice, automaticSigning, provisioningProfile, provisioningProfileSpecifier);
            config.AddProperty("ENABLE_STRICT_OBJC_MSGSEND", "YES");
            config.AddProperty("ENABLE_TESTABILITY", "YES");
            config.AddProperty("GCC_C_LANGUAGE_STANDARD", "gnu99");
            config.AddProperty("GCC_DYNAMIC_NO_PIC", "NO");
            config.AddProperty("GCC_NO_COMMON_BLOCKS", "YES");
            config.AddProperty("GCC_OPTIMIZATION_LEVEL", "0");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "DEBUG=1");
            config.AddProperty("GCC_PREPROCESSOR_DEFINITIONS", "$(inherited)");
            
            config.AddProperty("MTL_ENABLE_DEBUG_INFO", "YES");
            config.AddProperty("ONLY_ACTIVE_ARCH", "YES");
            
            config.AddProperty("SKIP_INSTALL", "YES");
        }

        // JASON: this is a modified copy of the AddFileImpl method
        public string AddFileCustom(string path, string projectPath, PBXSourceTree tree, bool isFolderReference, bool includeInIndex = true)
        {
            path = PBX.Utils.FixSlashesInPath(path);
            projectPath = PBX.Utils.FixSlashesInPath(projectPath);

            if (!isFolderReference && Path.GetExtension(path) != Path.GetExtension(projectPath))
                throw new Exception("Project and real path extensions do not match");

            string guid = FindFileGuidByProjectPath(projectPath);
            if (guid == null)
                guid = FindFileGuidByRealPath(path);
            if (guid == null)
            {
                PBXFileReferenceData fileRef;
                if (isFolderReference)
                {
                    fileRef = PBXFileReferenceData.CreateFromFolderReference(path, PBX.Utils.GetFilenameFromPath(projectPath), tree);
                }
                else
                {
                    // JASON: the main difference of this AddFileCustom to AddFileImpl is this next part that strips the full path back to the file name to be relative to the group
                    if (tree == PBXSourceTree.Group)
                    {
                        fileRef = PBXFileReferenceData.CreateFromFile(PBX.Utils.GetFilenameFromPath(projectPath), PBX.Utils.GetFilenameFromPath(projectPath), tree);
                    }
                    else
                    {
                        fileRef = PBXFileReferenceData.CreateFromFile(path, PBX.Utils.GetFilenameFromPath(projectPath), tree);
                    }
                    // JASON: also added a little hack to exclude a file from being indexed
                    if (!includeInIndex)
                    {
                        fileRef.ExcludeFromIndex();
                    }
                }
                PBXGroupData parent = CreateSourceGroup(PBX.Utils.GetDirectoryFromPath(projectPath));
                parent.children.AddGUID(fileRef.guid);
                FileRefsAdd(path, projectPath, parent, fileRef);
                guid = fileRef.guid;
            }
            return guid;
        }


        // ------ UTILS

        public static void CopyDirectory(string fromFolder, string toFolder, bool overwriteFiles)
        {
            fromFolder = Path.GetFullPath(fromFolder);
            toFolder = Path.GetFullPath(toFolder);

            foreach (var fromSubFolder in Directory.GetDirectories(fromFolder, "*", SearchOption.AllDirectories))
                Directory.CreateDirectory(fromSubFolder.Replace(fromFolder, toFolder));
            foreach (var fromFilePath in Directory.GetFiles(fromFolder, "*", SearchOption.AllDirectories))
            {
                var toFilePath = fromFilePath.Replace(fromFolder, toFolder);
                if (overwriteFiles)
                    File.Copy(fromFilePath, toFilePath, true);
                else if (!File.Exists(toFilePath))
                    File.Copy(fromFilePath, toFilePath);
            }
        }

        public static void CreateFolderPath(string path)
        {
            // creates the folder path if it doesnt exist
            path = FixSlashes(path);
            //Debug.Log ("* test: "+path+", "+Directory.Exists( path ));
            if (!Directory.Exists(path) && (path != "") && (path != "/"))
            {
                //Debug.Log ("* doesnt exist: "+path);
                System.IO.Directory.CreateDirectory(path);
            }
        }

        public static string FixSlashes(string s)
        {
            const string forwardSlash = "/";
            const string backSlash = "\\";
            return s.Replace(backSlash, forwardSlash);
        }
    }

} // namespace UnityEditor.iOS.Xcode
