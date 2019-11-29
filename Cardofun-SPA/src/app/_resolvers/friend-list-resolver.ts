import { Injectable } from '@angular/core';
import { Resolve, ActivatedRouteSnapshot, Router } from '@angular/router';
import { User } from '../_models/user';
import { AlertifyService } from '../_services/alertify/alertify.service';
import { Observable, of } from 'rxjs';
import { catchError } from 'rxjs/operators';
import { FriendService } from '../_services/friend/friend.service';
import { SupscriptionState } from '../_models/enums/supscriptionState';
import { UserFilterParams } from '../_models/userFilterParams';

@Injectable()
export class FriendListResolver implements Resolve<User[]> {
    pageNumber = 1;
    pageSize = 5;

    constructor(private friendService: FriendService, private alertifyService: AlertifyService,
        private router: Router) {}

    resolve(route: ActivatedRouteSnapshot): Observable<User[]> {
        const state: SupscriptionState = route.params['state'];
        let params: UserFilterParams = null;

        if (state) {
            params = { subscriptionState: state };
        }

        return this.friendService.getFriends(this.pageNumber, this.pageSize, params).pipe(
            catchError(error => {
                this.alertifyService.error('Problem retreiving data');
                this.router.navigate(['/home']);
                return of(null);
            })
        );
    }
}