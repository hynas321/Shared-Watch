export class HttpUrlHelper {
  static getRoomHash(url: string): string {
    const lastSlashIndex = url.lastIndexOf("/");
    return url.substring(lastSlashIndex + 1);
  }
}
