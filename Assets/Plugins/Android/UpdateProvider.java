package se.vibing.serenity;

import android.content.ContentProvider;
import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.MatrixCursor;
import android.net.Uri;
import android.os.ParcelFileDescriptor;
import android.provider.OpenableColumns;
import java.io.File;
import java.io.FileNotFoundException;

// Serves exactly one file, read-only: the downloaded update APK, so the system installer can read it (no androidx, so no FileProvider).
public class UpdateProvider extends ContentProvider {
    public static final String AUTHORITY = "se.vibing.serenity.updateprovider";

    // Where Updater.cs downloads to (also called from C#).
    public static String apkPath(Context c) {
        File dir = c.getExternalFilesDir(null);
        if (dir == null) dir = c.getFilesDir();
        return new File(dir, "update.apk").getAbsolutePath();
    }

    public static Uri apkUri() { return Uri.parse("content://" + AUTHORITY + "/update.apk"); }

    @Override public boolean onCreate() { return true; }

    @Override public ParcelFileDescriptor openFile(Uri uri, String mode) throws FileNotFoundException {
        if (!"update.apk".equals(uri.getLastPathSegment()) || !"r".equals(mode)) throw new FileNotFoundException();
        return ParcelFileDescriptor.open(new File(apkPath(getContext())), ParcelFileDescriptor.MODE_READ_ONLY);
    }

    @Override public Cursor query(Uri uri, String[] projection, String sel, String[] args, String order) {
        File f = new File(apkPath(getContext()));
        MatrixCursor cur = new MatrixCursor(new String[] { OpenableColumns.DISPLAY_NAME, OpenableColumns.SIZE });
        cur.addRow(new Object[] { "update.apk", f.length() });
        return cur;
    }

    @Override public String getType(Uri uri) { return "application/vnd.android.package-archive"; }
    @Override public Uri insert(Uri uri, ContentValues v) { return null; }
    @Override public int delete(Uri uri, String s, String[] a) { return 0; }
    @Override public int update(Uri uri, ContentValues v, String s, String[] a) { return 0; }
}
