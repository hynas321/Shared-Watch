export class HttpUrlHelper {
  getRoomHash(url: string) {
    const lastSlashIndex = url.lastIndexOf('/');
    return url.substring(lastSlashIndex + 1);
  }
}