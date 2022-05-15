﻿using ChunkyImageLib;
using ChunkyImageLib.DataHolders;
using PixiEditor.ChangeableDocument.Changeables;
using PixiEditor.ChangeableDocument.ChangeInfos;
using PixiEditor.ChangeableDocument.ChangeInfos.Drawing;

namespace PixiEditor.ChangeableDocument.Changes.Drawing;
internal static class DrawingChangeHelper
{
    public static ChunkyImage GetTargetImage(Document target, Guid memberGuid, bool drawOnMask)
    {
        var member = target.FindMemberOrThrow(memberGuid);
        if (drawOnMask)
        {
            if (member.Mask is null)
                throw new InvalidOperationException("Trying to draw on a mask that doesn't exist");
            return member.Mask;
        }
        else if (member is Folder)
        {
            throw new InvalidOperationException("Trying to draw on a folder");
        }
        return ((Layer)member).LayerImage;
    }

    public static void ApplyClipsSymmetriesEtc(Document target, ChunkyImage targetImage, Guid targetMemberGuid, bool drawOnMask)
    {
        if (!target.Selection.IsEmptyAndInactive)
            targetImage.AddRasterClip(target.Selection.SelectionImage);

        var targetMember = target.FindMemberOrThrow(targetMemberGuid);
        if (targetMember is Layer layer && layer.LockTransparency && !drawOnMask)
            targetImage.EnableLockTransparency();

        if (target.HorizontalSymmetryAxisEnabled)
            targetImage.SetHorizontalAxisOfSymmetry(target.HorizontalSymmetryAxisY);
        if (target.VerticalSymmetryAxisEnabled)
            targetImage.SetVerticalAxisOfSymmetry(target.VerticalSymmetryAxisX);
    }


    public static IChangeInfo CreateChunkChangeInfo(Guid memberGuid, HashSet<VecI> affectedChunks, bool drawOnMask)
    {
        return drawOnMask switch
        {
            false => new LayerImageChunks_ChangeInfo()
            {
                Chunks = affectedChunks,
                LayerGuid = memberGuid
            },
            true => new MaskChunks_ChangeInfo()
            {
                Chunks = affectedChunks,
                MemberGuid = memberGuid
            },
        };
    }
}
