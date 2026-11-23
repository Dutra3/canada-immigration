import { Injectable } from '@angular/core';
import { Meta, Title } from '@angular/platform-browser';
import { ActivatedRouteSnapshot, RouterStateSnapshot, TitleStrategy } from '@angular/router';

const DEFAULT_TITLE = 'Canada Immigration Tracker';
const TITLE_SUFFIX = ` | ${DEFAULT_TITLE}`;

@Injectable({ providedIn: 'root' })
export class AppTitleStrategy extends TitleStrategy {
  constructor(
    private readonly title: Title,
    private readonly meta: Meta
  ) {
    super();
  }

  override updateTitle(snapshot: RouterStateSnapshot): void {
    let route: ActivatedRouteSnapshot | null = snapshot.root;
    while (route?.firstChild) {
      route = route.firstChild;
    }

    const pageTitle = route?.data['title'] as string | undefined;
    const description = route?.data['description'] as string | undefined;

    this.title.setTitle(pageTitle ? `${pageTitle}${TITLE_SUFFIX}` : DEFAULT_TITLE);

    if (description) {
      this.meta.updateTag({ name: 'description', content: description });
    }
  }
}
